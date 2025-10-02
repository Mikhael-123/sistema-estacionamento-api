using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Dominio.Entidades;

public class Veiculo
{
  // Define a propriedade como chave primária
  [Key]
  // Define que a propriedade vai se autoincrementar se não for passado valor
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }
  // Define a propriedade como campo obrigatório
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