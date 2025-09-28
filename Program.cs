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

public class LoginDTO
{
  // `default!;` diz que a propriedade não é nullable
  public string Email { get; set; } = default!;
  public string Senha { get; set; } = default!;
}