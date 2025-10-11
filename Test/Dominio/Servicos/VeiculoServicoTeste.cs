using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

namespace Test.Dominio.Servicos;

[TestClass]
[DoNotParallelize]
public class VeiculoServicoTeste()
{
  private const string category = "VeiculoServico";
  private static DbContexto CriarDbContextoTeste()
  {
    // Cria uma configuração do builder para usar o arquivo "appsettings.json" do projeto "Test"
    var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").AddEnvironmentVariables();
    var configuration = builder.Build();

    // Cria uma instância de `DbContexto` com as configurações definidas para acessar o banco de dados
    var contexto = new DbContexto(configuration);
    // Apaga os dados do banco de dados para deixar pronto para teste
    contexto.Database.ExecuteSqlRaw("TRUNCATE TABLE veiculos");

    return contexto;
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se um veiculo criado é salvo no banco de dados")]
  public void TestandoSalvarVeiculo()
  {
    // Cria uma instância de `Veiculo`
    Veiculo novoVeiculo = new Veiculo
    {
      Id = 1,
      Nome = "veiculo1",
      Marca = "marca1",
      Ano = 2006
    };

    var contexto = CriarDbContextoTeste();
    var veiculoServico = new VeiculoServico(contexto);
    // Adiciona `novoVeiculo` no banco de dados
    veiculoServico.Incluir(novoVeiculo);
    // Retorna o veículo do banco de dados pelo id, ou nulo se não existir
    Veiculo? veiculoBanco = veiculoServico.BuscaPorId(1);

    // Verifica se o veículo de id 1 foi retornado do banco de dados
    Assert.IsNotNull(veiculoBanco, "O veículo de id 1 deveria ser retornado do banco de dados");
    // Verifica se as propriedades do veículo retornado do banco, são iguais as do veículo criado
    Assert.AreEqual(1, veiculoBanco.Id, "A propriedade 'Id' deve ser igual a propridade do veículo criado");
    Assert.AreEqual("veiculo1", veiculoBanco.Nome, "A propriedade 'Nome' deve ser igual a propridade do veículo criado");
    Assert.AreEqual("marca1", veiculoBanco.Marca, "A propriedade 'Marca' deve ser igual a propridade do veículo criado");
    Assert.AreEqual(2006, veiculoBanco.Ano, "A propriedade 'Ano' deve ser igual a propridade do veículo criado");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se todos os veiculos registrados no banco são retornados")]
  public void TestandoRetornarTodosVeiculos()
  {
    // Cria 3 instâncias de `Veiculos`
    Veiculo novoVeiculo1 = new Veiculo
    {
      Id = 1,
      Nome = "veiculo1",
      Marca = "marca1",
      Ano = 2006
    };
    Veiculo novoVeiculo2 = new Veiculo
    {
      Id = 2,
      Nome = "veiculo2",
      Marca = "marca2",
      Ano = 2006
    };
    Veiculo novoVeiculo3 = new Veiculo
    {
      Id = 3,
      Nome = "veiculo3",
      Marca = "marca3",
      Ano = 2006
    };

    var contexto = CriarDbContextoTeste();
    var veiculoServico = new VeiculoServico(contexto);
    // Adiciona `novoVeiculo1`, `novoVeiculo2` e `novoVeiculo3` no banco de dados
    veiculoServico.Incluir(novoVeiculo1);
    veiculoServico.Incluir(novoVeiculo2);
    veiculoServico.Incluir(novoVeiculo3);

    // Verifica se o tamanho da lista que o método `Todos()` retorna, é igual a quantidade de veículos criados
    Assert.AreEqual(3, veiculoServico.Todos().Count(), "A quantidade de veículos no banco de dados deveria ser igual a de veículos criados");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se um veiculo é apagado do banco de dados")]
  public void TestandoApagarVeiculo()
  {
    // Cria uma instância de `Veiculo`
    Veiculo novoVeiculo = new Veiculo
    {
      Id = 1,
      Nome = "veiculo1",
      Marca = "marca1",
      Ano = 2006
    };

    var contexto = CriarDbContextoTeste();
    var veiculoServico = new VeiculoServico(contexto);
    // Adiciona `novoVeiculo` no banco de dados
    veiculoServico.Incluir(novoVeiculo);
    // Retorna o veículo do banco de dados pelo id, ou nulo se não existir
    Veiculo? veiculoBanco = veiculoServico.BuscaPorId(1);
    // Verifica se o veículo retornado não é nulo
    Assert.IsNotNull(veiculoBanco, "O veículo de id 1 deveria ser retornado do banco de dados");

    // Apaga o veículo do banco
    veiculoServico.Apagar(veiculoBanco);
    // Retorna o veículo do banco de dados pelo id, ou nulo se não existir
    veiculoBanco = veiculoServico.BuscaPorId(1);
    // Verifica se o veículo de id 1 foi apagado do banco de dados
    Assert.IsNull(veiculoBanco, "O veículo de id 1 deveria ser apagado do banco de dados");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se um veículo é atualizado")]
  public void TestandoAtualizarVeiculo()
  {
    Veiculo novoVeiculo = new Veiculo
    {
      Id = 1,
      Nome = "veiculo1",
      Marca = "marca1",
      Ano = 2006
    };

    var contexto = CriarDbContextoTeste();
    var veiculoServico = new VeiculoServico(contexto);
    veiculoServico.Incluir(novoVeiculo);
    Veiculo? veiculoBanco = veiculoServico.BuscaPorId(1);

    Assert.IsNotNull(veiculoBanco);
    // Verifica se as propriedades do veículo retornado do banco, são iguais as do veículo criado
    Assert.AreEqual(1, veiculoBanco.Id, "A propriedade 'Id' deve ser igual a propridade do veículo criado");
    Assert.AreEqual("veiculo1", veiculoBanco.Nome, "A propriedade 'Nome' deve ser igual a propridade do veículo criado");
    Assert.AreEqual("marca1", veiculoBanco.Marca, "A propriedade 'Marca' deve ser igual a propridade do veículo criado");
    Assert.AreEqual(2006, veiculoBanco.Ano, "A propriedade 'Ano' deve ser igual a propridade do veículo criado");

    novoVeiculo.Nome = "veiculo1Atualizado";
    novoVeiculo.Marca = "marca1Atualizada";
    novoVeiculo.Ano = 2010;
    veiculoServico.Atualizar(novoVeiculo);
    veiculoBanco = veiculoServico.BuscaPorId(1);

    Assert.IsNotNull(veiculoBanco);
    // Verifica se as propriedades do veículo retornado do banco, são iguais as do veículo atulizado
    Assert.AreEqual(1, veiculoBanco.Id, "A propriedade 'Id' deve ser igual a propridade do veículo atualizado");
    Assert.AreEqual("veiculo1Atualizado", veiculoBanco.Nome, "A propriedade 'Nome' deve ser igual a propridade do veículo atualizado");
    Assert.AreEqual("marca1Atualizada", veiculoBanco.Marca, "A propriedade 'Marca' deve ser igual a propridade do veículo atualizado");
    Assert.AreEqual(2010, veiculoBanco.Ano, "A propriedade 'Ano' deve ser igual a propridade do veículo atualizado");
  }
}