using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

# region Builder
// Variável que prepara a aplicação web definindo serviços antes de "construir" a aplicação
var builder = WebApplication.CreateBuilder(args);

// Adicionando swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona o serviço de autorização de rotas
builder.Services.AddAuthorization();

// Pega a chave JWT do json de "appsettings.json"
var jwtKey = builder.Configuration.GetSection("Jwt")["Key"];
// Lança um erro se não existir uma chave JWT no "appsettings.json"
if (string.IsNullOrEmpty(jwtKey))
  throw new Exception("Não foi possível pegar a chave JWT");
  
// Adicionando o serviõ de autenticação por tokens JWT
builder.Services.AddAuthentication(option => {
  option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
  option.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
  };
});

// Diz que ao chamar `IAdministradorServico`, é para criar uma instância de `AdministradorServico`
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
// Diz que ao chamar `IVeiculoServico`, é para criar uma instância de `VeiculoServico`
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

// Configura o `builder` para conectar ao banco de dados
builder.Services.AddDbContext<DbContexto>(opt =>
{
  // Pega a propriedade "mysql" da "ConnectionStrings" do arquivo appsettings.json
  var stringConexao = builder.Configuration.GetConnectionString("MySql");
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

// Usa a autenticação e autorização de rotas respectivamente *trocar a ordem pode dar erro*
app.UseAuthentication();
// OBS: É necessário adicionar o serviço `AddAuthorization` no `builder` também
app.UseAuthorization();
# endregion

# region Home
// Rota padrão da aplicação, retorna um json com algumas informações
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
# endregion

# region Administradores
// Loga o usuário como admnistrador se as credenciais estiverem corretas
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  Administrador? adm = administradorServico.Login(loginDTO);

  if (adm != null)
  {
    var token = GerarTokenJWT(adm);
    
    return Results.Ok(new AdministradorLogado
    {
      Email = adm.Email,
      Perfil = adm.Perfil,
      Token = token,
    });
  }
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
}).WithTags("Administradores").RequireAuthorization();

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
  // `administradores` guarda todos os dados dos administradores do banco dados
  List<Administrador> administradores = administradorServico.Todos(pagina);
  // `administradoresView` guarda dados de administradores que podem ser mostrados ao usuário
  List<AdministradorModelView> administradoresView = PegarAdministradoresView(administradores);

  return Results.Ok(administradoresView);
}).WithTags("Administradores").RequireAuthorization();

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
}).WithTags("Administradores").RequireAuthorization();
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
}).WithTags("Veiculos").RequireAuthorization();

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
}).WithTags("Veiculos").RequireAuthorization();

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
}).WithTags("Veiculos").RequireAuthorization();

// Deleta um veículo pelo Id especificado na rota
app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{ 
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos").RequireAuthorization();
# endregion

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

// Gera e retorna um Json Web Token
string GerarTokenJWT(Administrador administrador)
{
  // `securityKey` guarda a criptografia da chave JWT
  var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
  // Gera uma credencial passando a chave criptografada e o algoritmo de descriptografia, e guarda em `credentials`
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  // Cria uma lista de `Claim`, cada `Claim` é um dado do Json Web Token
  var claims = new List<Claim>
  {
    new Claim(ClaimTypes.Email, administrador.Email),
    new Claim("Perfil", administrador.Perfil),
  };

  // Cria um token com a lista de `Claim`, com duração de 1 dia e com as credenciais geradas
  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
}
# endregion

app.Run();