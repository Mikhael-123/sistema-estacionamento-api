using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;

// Herda a classe `DbContext` do Entity Framework para configurar o ORM
public class DbContexto : DbContext
{
  // Variável que lê o arquivo appsettings.json
  private readonly IConfiguration _configuracaoAppSettings;
  public DbContexto(IConfiguration configurationAppSettings)
  {
    _configuracaoAppSettings = configurationAppSettings;
  }

  // Cria a entidade `Administradores` no banco de dados
  public DbSet<Administrador> Administradores { get; set; } = default!;
  // Cria a entidade `Veiculos` no banco de dados
  public DbSet<Veiculo> Veiculos { get; set; } = default!;
  // Sobrescreve o método `OnModelCreating` da classe `DbContext` para criar um administrador padrão
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Administrador>().HasData(
      new Administrador
      {
        // A propriedade `Id` precisa ser especificada durante a seed, mesmo que ela seja "auto-incrementada"
        Id = 1,
        Email = "adm@teste.com",
        Senha = "123456",
        Perfil = "Adm"
      }
    );
  }
  // Sobrescreve o método `OnConfiguring` da classe `DbContext` para conectar ao banco
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // Se em "Program.cs" não foi configurado o banco de dados no `builder`, ele é configurado neste bloco de código
    if (!optionsBuilder.IsConfigured)
    {
      // Pega a propriedade "mysql" de "ConnectionStrings" do json no arquivo appsettings.json
      string? stringConexao = _configuracaoAppSettings.GetConnectionString("mysql");
      if (!string.IsNullOrEmpty(stringConexao))
      {
        // Faz a conexão com o banco usando a string de conexão
        optionsBuilder.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
      }
    }
  }
}