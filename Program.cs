using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

# region Utils
// Função genérica que verifica a instância do DTO recebido e valida os dados
ErrosDeValidacao validaDTO<T>(T dto)
{
  // Instanciando a classe `ErrosDeValidacao` para acumular as mensagens de erro de validação
  // A propriedade `Mensagens`, deve ser instanciada
  var validacao = new ErrosDeValidacao()
  {
    Mensagens = new List<string>()
  };

  // Verificando de qual instância o DTO recebido pertence, e validando as propriedades do DTO recebido
  if (dto is VeiculoDTO veiculoDTO)
  {
    if (string.IsNullOrEmpty(veiculoDTO.Nome))
      validacao.Mensagens.Add("O campo 'nome' não pode ficar em branco");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
      validacao.Mensagens.Add("O campo 'marca' não pode ficar em branco");

    if (veiculoDTO.Ano < 1950)
      validacao.Mensagens.Add("O campo 'ano' não pode ser inferior à 1950");
  }
  else if (dto is AdministradorDTO administradorDTO)
  {
    if (string.IsNullOrEmpty(administradorDTO.Email))
      validacao.Mensagens.Add("O campo 'email' não pode ficar em branco");
    if (string.IsNullOrEmpty(administradorDTO.Senha))
      validacao.Mensagens.Add("O campo 'senha' não pode ficar em branco");
    if (string.IsNullOrEmpty(administradorDTO.Perfil.ToString()))
      validacao.Mensagens.Add("O campo 'perfil' não pode ficar em branco");
  }
    
  return validacao;
}

// Recebe uma lista com todos os dados dos administradores do banco de dados e retorna uma lista com somente as informações permitidas
List<AdministradorModelView> PegarAdministradoresView(List<Administrador> administradoresModel)
{
  // O parâmetro `administradoresModel` é uma lista de instâncias de `Administrador`, que possuem todos os dados relacionados aos administradores do banco de dados, `AdministradorModelView` é um objeto para mostrar apenas as informações permitidas ao usuário
  List<AdministradorModelView> administradoresView = new List<AdministradorModelView>();

  // Pegando os dados de `administradoresModel` e colocando em `administradoresView` para que sejam mostradas apenas as informações permitidas ao usuário
  foreach (Administrador administrador in administradoresModel)
  {
    AdministradorModelView administradorView = new AdministradorModelView
    {
      Id = administrador.Id,
      Email = administrador.Email,
      Perfil = administrador.Perfil,
    };

    administradoresView.Add(administradorView);
  }

  // Retorna uma lista de `AdministradorModelView`, que mostram apenas as informações de administradores permitidas
  return administradoresView;
}
# endregion

# region Builder
// Variável que prepara a aplicação web definindo serviços antes de "construir" a aplicação
var builder = WebApplication.CreateBuilder(args);

// Adicionando swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Diz que ao chamar `IAdministradorServico`, é para criar uma instância de `AdministradorServico`
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
// Diz que ao chamar `IVeiculoServico`, é para criar uma instância de `VeiculoServico`
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

// Configura o `builder` para conectar ao banco de dados
builder.Services.AddDbContext<DbContexto>(opt =>
{
  // Pega a propriedade "mysql" da "ConnectionStrings" do arquivo appsettings.json
  var stringConexao = builder.Configuration.GetConnectionString("mysql");
  // Conecta ao banco de dados MySQL
  opt.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
});
# endregion

# region App
// Cria uma instância de `builder`
var app = builder.Build();
// Usa o swagger se a aplicação tiver em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
# endregion

# region Home
// Rota padrão da aplicação, retorna um json com algumas informações
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
# endregion

# region Administradores
// Loga o usuário como admnistrador se as credenciais estiverem corretas
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  if (administradorServico.Login(loginDTO) != null)
    return Results.Ok("Login realizado com sucesso");
  else
    return Results.Unauthorized();
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
  ErrosDeValidacao validacao = validaDTO(administradorDTO);
  // Se a quantidade de strings dentro de `Mensagens` for maior que 0, é retornado um "BadRequest"
  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  Administrador administrador = new Administrador
  {
    Email = administradorDTO.Email,
    Senha = administradorDTO.Senha,
    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(),
  };
  administradorServico.Incluir(administrador);

  var administradorView = new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil,
  };

  return Results.Created($"/administradores/{administradorView.Id}", administradorView);
}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
  // `administradores` guarda todos os dados dos administradores do banco dados
  List<Administrador> administradores = administradorServico.Todos(pagina);
  // `administradoresView` guarda dados de administradores que podem ser mostrados ao usuário
  List<AdministradorModelView> administradoresView = PegarAdministradoresView(administradores);

  return Results.Ok(administradoresView);
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromQuery] int id, IAdministradorServico administradorServico) =>
{
  Administrador? administrador = administradorServico.BuscaPorId(id);

  if (administrador == null) return Results.NotFound();

  var administradorView = new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil,
  };

  return Results.Ok(administrador);
}).WithTags("Administradores");
# endregion

# region Veiculos
// Cadastra um novo veículo, pegando as informações pelo body da requisição
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  ErrosDeValidacao validacao = validaDTO(veiculoDTO);
  // Se a quantidade de strings dentro de `Mensagens` for maior que 0, é retornado um "BadRequest"
  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);

  Veiculo veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano,
  };
  veiculoServico.Incluir(veiculo);

  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

// Retorna alguns veículos cadastrados separados por página, que pode ser especificada na query
app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
  List<Veiculo> veiculos = veiculoServico.Todos(pagina);

  return Results.Ok(veiculos);
}).WithTags("Veiculos");

// Retorna 1 veículo pelo Id especificado na rota
app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

// Atualiza um veículo pelo Id especificado na rota
app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  ErrosDeValidacao validacao = validaDTO(veiculoDTO);
  // Se a quantidade de strings dentro de `Mensagens` for maior que 0, é retornado um "BadRequest"
  if (validacao.Mensagens.Count > 0)
    return Results.BadRequest(validacao);
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoServico.Atualizar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos");

// Deleta um veículo pelo Id especificado na rota
app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{ 
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos");
# endregion

app.Run();