using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Dominio.DTOs;

public record VeiculoDTO
{
  [Required]
  // Define 150 como limite de tamanho da propriedade
  [StringLength(150)]
  public string Nome { get; set; } = default!;
  [Required]
  // Define 100 como limite de tamanho da propriedade
  [StringLength(100)]
  public string Marca { get; set; } = default!;
  [Required]
  public int Ano { get; set; } = default!;
}