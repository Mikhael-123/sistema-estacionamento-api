namespace MinimalApi.Dominio.DTOs;

public class LoginDTO
{
  // `default!;` diz que a propriedade não é nullable
  public string Email { get; set; } = default!;
  public string Senha { get; set; } = default!;
}