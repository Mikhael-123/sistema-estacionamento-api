using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

namespace Test.Dominio.Servicos;

[TestClass]
// Os testes de "MSTest" são executados simultâneamente, como os testes de `AdministradorServicoTeste` estão programados para sempre apagar os dados do banco de dados do teste anterior, é preciso desativar essa função pois se não os testes irão falhar ao serem executados juntos
[DoNotParallelize]
public sealed class AdministradorServicoTeste
{
  // Cria uma configuração de `DbContext` para teste e retorna uma instância de `DbContexto` com essas configurações
  private static DbContexto CriarDbContextoTeste()
  {
    // Configurar o ConfigurationBuilder
    var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
    var configuration = builder.Build();

    var contexto = new DbContexto(configuration);
    // Apaga todos os registros da tabela administradores
    contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

    return contexto;
  }

  [TestMethod]
  [TestCategory("AdministradorServico")]
  [Description("Verifica se um administrador criado é salvo no banco de dados")]
  public void TestandoSalvarNovoAdministrador()
  {
    // Cria um administrador
    var adm = new Administrador();
    adm.Id = 1;
    adm.Email = "teste@teste.com";
    adm.Senha = "teste123";
    adm.Perfil = "Adm";

    var contexto = CriarDbContextoTeste();
    // Cria uma instância da classe de serviço dos administradores
    var administradorServico = new AdministradorServico(contexto);
    // Adiciona o administrador criado
    administradorServico.Incluir(adm);

    // Verifica existe 1 registro em administradores
    Assert.AreEqual(1, administradorServico.Todos().Count(), "Deveria haver 1 registro na tabela administradores");
  }

  [TestMethod]
  [TestCategory("AdministradorServico")]
  [Description("Verifica se um administrador salvo no banco de dados é retornado pelo id")]
  public void TestandoBuscaPorId()
  {
    // Cria um administrador
    var adm = new Administrador
    {
      Id = 1,
      Email = "teste@teste.com",
      Senha = "teste123",
      Perfil = "Adm",
    };

    var contexto = CriarDbContextoTeste();
    // Cria uma instância da classe de serviço dos administradores
    var administradorServico = new AdministradorServico(contexto);
    // Adiciona o administrador criado
    administradorServico.Incluir(adm);
    // Busca o administrador criado no banco de dados
    Administrador? newAdm = administradorServico.BuscaPorId(adm.Id);

    // Verifica se `newAdm` não é nulo
    Assert.IsNotNull(newAdm, "Deveria retornar um administrador do banco de dados");
    // Verifica se o id de `administrador` é o mesmo do administrador criado no teste
    Assert.AreEqual(1, newAdm.Id, "Deveria retornar o administrador de id 1 no banco de dados");
  }

  [TestMethod]
  [TestCategory("AdministradorServico")]
  [Description("Verifica se um administrador de perfil 'Adm' salvo no banco de dados é retornado ao passar seu email e senha")]
  public void TestandoLogin()
  {
    // Cria um novo administrador
    var newAdm = new Administrador
    {
      Id = 1,
      Email = "teste@teste.com",
      Senha = "teste123",
      Perfil = "Adm",
    };

    var contexto = CriarDbContextoTeste();
    var administradorServico = new AdministradorServico(contexto);
    // Insere um novo administrador no banco de dados
    administradorServico.Incluir(newAdm);

    // É instanciado uma classe que vai passar os dados de login do administrador
    LoginDTO loginDTO = new LoginDTO
    {
      Email = newAdm.Email,
      Senha = newAdm.Senha
    };

    // É passado o Email e a Senha do administrador salvo no banco, e é retonado na variável `administrador`
    Administrador? administrador = administradorServico.Login(loginDTO);

    // Verifica se `administrador` não é nulo
    Assert.IsNotNull(administrador, "Deveria ser retornado um administrador do banco de dados");
    // Verifica se o id de `administrador` é o mesmo do administrador criado no teste
    Assert.AreEqual(1, administrador.Id, "Deveria retornar o administrador de id 1 do banco de dados");
  }

  [TestMethod]
  [TestCategory("AdministradorServico")]
  [Description("Verifica se um administrador de perfil 'Adm' salvo no banco de dados é retornado ao passar seu email e senha")]
  public void TestandoRetonarTodosAdministradores()
  {
    // Criando novos administradores
    var newAdm1 = new Administrador
    {
      Id = 1,
      Email = "teste1@teste.com",
      Senha = "teste123",
      Perfil = "Adm"
    };
    var newAdm2 = new Administrador
    {
      Id = 2,
      Email = "teste2@teste.com",
      Senha = "teste123",
      Perfil = "Adm"
    };
    var newAdm3 = new Administrador
    {
      Id = 3,
      Email = "teste3@teste.com",
      Senha = "teste123",
      Perfil = "Adm"
    };

    var contexto = CriarDbContextoTeste();
    var administradorServico = new AdministradorServico(contexto);
    // Adicionando os novos administradores no banco
    administradorServico.Incluir(newAdm1);
    administradorServico.Incluir(newAdm2);
    administradorServico.Incluir(newAdm3);

    // Retona os administradores do banco
    List<Administrador> administradores = administradorServico.Todos();

    // Verifica se o tamanho da lista de administradores é igual ao número de novos administradoes criados e inseridos no banco de dados
    Assert.AreEqual(3, administradores.Count(), "Deveria ser retornado o número de administradores criados");
  }
}