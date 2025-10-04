using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Dominio.DTOs;

public record VeiculoDTO
{
  [Required]
  // Define 150 como limite de tamanho da propriedade
  [MaxLength(150)]
  [MinLength(1)]
  public string Nome { get; set; } = default!;
  [Required]
  // Define 100 como limite de tamanho da propriedade
  [MaxLength(100)]
  [MinLength(1)]
  public string Marca { get; set; } = default!;
  [Required]
  public int Ano { get; set; } = default!;
}