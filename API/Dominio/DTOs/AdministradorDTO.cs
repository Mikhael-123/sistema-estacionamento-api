using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MinimalApi.Dominio.Enuns;

namespace MinimalApi.Dominio.DTOs;

public class AdministradorDTO
{
  // `default!;` diz que a propriedade não é nullable
  // as anotações "data annotations" estão sendo usadas aqui, principalmente, com o intuito de documentar o DTO no swagger

  [Required]
  // Define que a propriedade `Email` é uma string que deve conter um endereço de email
  [EmailAddress]
  public string Email { get; set; } = default!;
  [Required]
  // Define 8 como tamanho minimo da propriedade
  [MinLength(8)]
  // Define 50 como limite de tamanho da propriedade
  [MaxLength(50)]
  public string Senha { get; set; } = default!;
  [Required]
  [Description("Retorna 'Adm' ou 'Editor'")]
  public Perfil Perfil { get; set; } = default!;
}