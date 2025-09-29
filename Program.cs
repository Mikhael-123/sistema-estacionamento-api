using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Infraestrutura.Db;

// Variável que prepara a aplicação web definindo serviços antes de "construir" a aplicação
var builder = WebApplication.CreateBuilder(args);
// Configura o `builder` para conectar ao banco de dados
builder.Services.AddDbContext<DbContexto>(opt =>
{
  // Pega a propriedade "mysql" da "ConnectionStrings" do arquivo appsettings.json
  var stringConexao = builder.Configuration.GetConnectionString("mysql");
  // Conecta ao banco de dados MySQL
  opt.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
});
// Cria uma instância de `builder`
var app = builder.Build();

// Define uma rota GET ao acessar "/"
app.MapGet("/", () => "Rota padrão");

// Define uma rota POST ao acessar "/login"
// OBS: O parâmetro irá pegar o JSON no BODY da requisição
app.MapPost("/login", (LoginDTO loginDTO) =>
{
  if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "12345678")
    return Results.Ok("Login realizado com sucesso");
  else
    return Results.Unauthorized();
});

app.Run();