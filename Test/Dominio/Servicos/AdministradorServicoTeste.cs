using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

namespace Test.Dominio.Servicos;

[TestClass]
public sealed class AdministradorServicoTeste
{
  // Cria uma configuração de `DbContext` para teste e retorna uma instância de `DbContexto` com essas configurações
  private DbContexto CriarDbContextoTeste()
  {
    // Configurar o ConfigurationBuilder
    var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
    var configuration = builder.Build();

    return new DbContexto(configuration);
  }

  [TestMethod]
  public void TestandoSalvarNovoAdministrador()
  {
    // Cria um administrador
    var adm = new Administrador();
    adm.Id = 1;
    adm.Email = "teste2@teste.com";
    adm.Senha = "teste123";
    adm.Perfil = "Adm";
    // Cria uma instância de `DbContexto` para teste
    var contexto = CriarDbContextoTeste();
    // Apaga todos os registros da tabela administradores
    contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

    // Cria uma instância da classe de serviço dos administradores
    var administradorServico = new AdministradorServico(contexto);
    // Adiciona o administrador criado
    administradorServico.Incluir(adm);

    // Verifica existe 1 registro em administradores
    Assert.AreEqual(1, administradorServico.Todos().Count());
  }

  [TestMethod]
  public void TestandoBuscaPorId()
  {
    // Cria um administrador
    var adm = new Administrador();
    adm.Email = "teste2@teste.com";
    adm.Senha = "teste123";
    adm.Perfil = "Adm";
    // Cria uma instância de `DbContexto` para teste
    var contexto = CriarDbContextoTeste();
    // Apaga todos os registros da tabela administradores
    contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

    // Cria uma instância da classe de serviço dos administradores
    var administradorServico = new AdministradorServico(contexto);
    // Adiciona o administrador criado
    administradorServico.Incluir(adm);
    // Busca o administrador criado no banco de dados
    var newAdm = administradorServico.BuscaPorId(adm.Id);

    // Verifica se `newAdm` não é nulo
    Assert.IsNotNull(newAdm);
    // Verifica se o administrador criado está no banco. O id do primeiro registro do banco deve ser 1 pois está como "auto-incremento"
    Assert.AreEqual(1, newAdm.Id);
  }
}