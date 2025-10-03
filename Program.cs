using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
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
// Define uma rota GET ao acessar "/"
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
# endregion

# region Administradores
// Define uma rota POST ao acessar "/login"
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  if (administradorServico.Login(loginDTO) != null)
    return Results.Ok("Login realizado com sucesso");
  else
    return Results.Unauthorized();
}).WithTags("Administradores");
# endregion

# region Veiculos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  Veiculo veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano,
  };
  veiculoServico.Incluir(veiculo);

  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
  List<Veiculo> veiculos = veiculoServico.Todos(pagina);

  return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoServico.Atualizar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{ 
  Veiculo? veiculo = veiculoServico.BuscaPorId(id);

  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);

  return Results.NoContent();
}).WithTags("Veiculos");
# endregion
app.Run();