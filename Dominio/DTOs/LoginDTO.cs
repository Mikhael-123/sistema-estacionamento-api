using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Dominio.DTOs;

public class LoginDTO
{
  // `default!;` diz que a propriedade não é nullable
  // as anotações "data annotations" estão sendo usadas aqui, principalmente, com o intuito de documentar o DTO no swagger

  [Required]
  [EmailAddress]
  public string Email { get; set; } = default!;
  [Required]
  [MinLength(8)]
  [MaxLength(50)]
  public string Senha { get; set; } = default!;
}