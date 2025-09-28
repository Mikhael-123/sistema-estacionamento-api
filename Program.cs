using MinimalAPI.Dominio.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Rota padrão");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
  if (loginDTO.Email == "adm@teste.com" && loginDTO.Senha == "12345678")
    return Results.Ok("Login realizado com sucesso");
  else
    return Results.Unauthorized();
});

app.Run();