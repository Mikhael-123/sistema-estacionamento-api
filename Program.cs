using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

// Variável que prepara a aplicação web definindo serviços antes de "construir" a aplicação
var builder = WebApplication.CreateBuilder(args);
// Adicionando swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*
`AddScoped`, `AddTransient` e `AddSingleton`, são métodos que executam um serviço (TService), ao chamar uma implementação (TImplementation), onde "TService" são as Interfaces que definem como a classe/implementação deve ser estruturada, e "TImplementation" é onde fica a lógica responsável por fazer o serviço funcionar

`AddScoped<TService, TImplementation>()` diz que ao chamar "TService", é para criar uma instância de "TImplementation" a cada *nova requisição*
`AddTransient<TService, TImplementation>()` diz que ao chamar "TService", é para criar uma instância de "TImplementation" *toda vez* que o *serviço é solicitado*
`AddSingleton<TService, TImplementation>()` diz que ao chamar "TService", é para criar uma instância *única* de "TImplementation" para *toda a aplicação*
*/

// Diz que ao chamar `IAdministradorServico`, é para criar uma instância de `AdministradorServico`
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
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

// Usa o swagger se a aplicação tiver em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// Define uma rota GET ao acessar "/"
app.MapGet("/", () => "Rota padrão");

// Define uma rota POST ao acessar "/login"
app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  if (administradorServico.Login(loginDTO) != null)
    return Results.Ok("Login realizado com sucesso");
  else
    return Results.Unauthorized();
});

app.Run();