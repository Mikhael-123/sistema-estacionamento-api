using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Dominio.Entidades;

public class Administrador
{
  // Define a propriedade como chave primária
  [Key]
  // Define que a propriedade vai se autoincrementar se não for passado valor
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Id { get; set; }
  // Define a propriedade como campo obrigatório
  [Required]
  // Define 255 como limite de tamanho da propriedade
  [StringLength(255)]
  public string Email { get; set; } = default!;
  [Required]
  // Define 50 como limite de tamanho da propriedade
  [StringLength(50)]
  public string Senha { get; set; } = default!;
  [Required]
  // Define 10 como limite de tamanho da propriedade
  [StringLength(10)]
  public string Perfil { get; set; } = default!;
}