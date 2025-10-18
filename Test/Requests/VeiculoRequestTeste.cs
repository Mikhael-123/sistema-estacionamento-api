using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using Test.Helpers;
using Test.Mocks;

namespace Test.Requests;

[TestClass]
[DoNotParallelize]
public class VeiculoRequestTeste
{
  // Inicia a classe `Setup` passando o `TestContext` de `VeiculoRequestTeste`
  [ClassInitialize]
  public static void ClassInitialize(TestContext context)
  {
    Setup.ClassInit(context);
  }
  [ClassCleanup]
  public static void ClassCleanup()
  {
    Setup.ClassCleanup();
  }

  // Limpa o "banco" antes e depois de cada teste
  [TestInitialize]
  public void TestInitialize()
  {
    VeiculoServicoMock.ApagarTudo();
  }
  [TestCleanup]
  public void TestCleanup()
  {
    VeiculoServicoMock.ApagarTudo();
  }

  private const string category = "VeiculoRequest";

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica o administrador padrão retornado de `AdministradorServicoMock` e se é criado um token JWT para ele")]
  public void TestandoTokenAdministradorPadrao()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);

    Assert.IsNotNull(admPadrao, "Deveria ser retornado um administrador padrão de `AdministradorServicoMock`");
    Assert.AreEqual(Perfil.Adm.ToString(), admPadrao.Perfil, "O administrador padrão deveria ter o perfil de administrador");

    string token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    Assert.AreNotEqual("", token, "Deveria ser retornado um token JWT");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se rota POST:/veiculos recebe e cria um veículo, além de retornar o caminho para acessar o veículo criado")]
  public async Task TestandoPostVeiculos()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);
    string token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    var veiculoServicoMock = new VeiculoServicoMock();

    var novoVeiculo = new VeiculoDTO
    {
      Nome = "veiculo1",
      Marca = "marca1",
      Ano = 2006
    };

    var request = new HttpRequestMessage(HttpMethod.Post, "/veiculos");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Content = new StringContent(JsonSerializer.Serialize(novoVeiculo), Encoding.UTF8, "application/json");
    var response = await Setup.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "A requisição deveria retornar o status http '201' (Created)");

    var content = await response.Content.ReadAsStringAsync();
    Veiculo? veiculo = JsonSerializer.Deserialize<Veiculo>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    Assert.IsNotNull(veiculo, "A requisição deveria retornar um veículo");
    Assert.AreEqual(1, veiculo.Id, "A propriedade 'Id' do veículo retornado da requisição deveria ser 1");
    Assert.AreEqual("/veiculos/1", response.Headers.Location?.ToString(), "A requisição deveria retorna o caminho para acessar o administrador criado");
    Assert.AreEqual(novoVeiculo.Nome, veiculo.Nome, "A propriedade 'Nome' do veículo retornado da requisição deve ser igual a do veículo criado");
    Assert.AreEqual(novoVeiculo.Marca, veiculo.Marca, "A propriedade 'Marca' do veículo retornado da requisição deve ser igual a do veículo criado");
    Assert.AreEqual(novoVeiculo.Ano, veiculo.Ano, "A propriedade 'Ano' do veículo retornado da requisição deve ser igual a do veículo criado");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se rota GET:/veiculos/{id} recebe e cria um veículo, além de retornar o caminho para acessar o veículo criado")]
  public async Task TestandoGetVeiculoPorId()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);
    string token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    var veiculoServicoMock = new VeiculoServicoMock();

    var novoVeiculo = new Veiculo
    {
      Nome = "veiculo1",
      Marca = "marca1",
      Ano = 2006
    };
    veiculoServicoMock.Incluir(novoVeiculo);

    var request = new HttpRequestMessage(HttpMethod.Get, "/veiculos/1");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var response = await Setup.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "A requisição deveria retornar o status code '200 (Ok)'");

    var content = await response.Content.ReadAsStringAsync();
    var veiculo = JsonSerializer.Deserialize<Veiculo>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.IsNotNull(veiculo, "A requisição deveria retornar um veículo");
    Assert.AreEqual(1, veiculo.Id, "A propriedade 'Id' do veículo retornado da requisição deveria ser '1'");
    Assert.AreEqual(novoVeiculo.Nome, veiculo.Nome, "A propriedade 'Nome' do veículo retornado da requisição deveria ser igual a do veículo criado");
    Assert.AreEqual(novoVeiculo.Marca, veiculo.Marca, "A propriedade 'Marca' do veículo retornado da requisição deveria ser igual a do veículo criado");
    Assert.AreEqual(novoVeiculo.Ano, veiculo.Ano, "A propriedade 'Ano' do veículo retornado da requisição deveria ser igual a do veículo criado");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se a rota PUT:/veiculos/{id} recebe um veículo e atualiza os dados do veículo de id especificado com as propriedades recebidas")]
  public async Task TestandoPutVeiculos()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);
    string token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    var veiculoServicoMock = new VeiculoServicoMock();

    var novoVeiculo = new Veiculo
    {
      Nome = "veiculo1Atualizar",
      Marca = "marca1Atualizar",
      Ano = 2006
    };
    veiculoServicoMock.Incluir(novoVeiculo);

    var veiculoBancoMock = veiculoServicoMock.BuscaPorId(1);

    Assert.IsNotNull(veiculoBancoMock, "Deveria ser retornado o veículo criado de `VeiculoServicoMock`");

    var propriedadesParaAtualizar = new VeiculoDTO
    {
      Nome = "veiculo1Atualizado",
      Marca = "marca1Atualizada",
      Ano = 2000
    };

    var request = new HttpRequestMessage(HttpMethod.Put, "/veiculos/1");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Content = new StringContent(JsonSerializer.Serialize(propriedadesParaAtualizar), Encoding.UTF8, "application/json");
    var response = await Setup.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "A requisição deveria retornar o status http '201 (No Content)'");

    veiculoBancoMock = veiculoServicoMock.BuscaPorId(1);

    Assert.IsNotNull(veiculoBancoMock);
    Assert.AreEqual(1, veiculoBancoMock.Id, "O veiculo de `VeiculoServicoMock` deveria ter o id 1");
    Assert.AreEqual("veiculo1Atualizado", veiculoBancoMock.Nome, "A propriedade do veículo de id 1 deveria ser atualizada com os dados enviados na requisição");
    Assert.AreEqual("marca1Atualizada", veiculoBancoMock.Marca, "A propriedade do veículo de id 1 deveria ser atualizada com os dados enviados na requisição");
    Assert.AreEqual(2000, veiculoBancoMock.Ano, "A propriedade do veículo de id 1 deveria ser atualizada com os dados enviados na requisição");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se a rota DELETE:/veiculos/{id} apaga o veículo de id especificado")]
  public async Task TestandoDeleteVeiculoPorId()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);
    string token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    var veiculoServicoMock = new VeiculoServicoMock();

    var novoVeiculo = new Veiculo
    {
      Nome = "deleteVeiculo1",
      Marca = "deleteMarca1",
      Ano = 2006
    };

    veiculoServicoMock.Incluir(novoVeiculo);
    var veiculoBancoMock = veiculoServicoMock.BuscaPorId(1);
    Assert.IsNotNull(veiculoBancoMock, "Deveria ser retornado o veiculo criado");
    Assert.AreEqual(novoVeiculo.Nome, veiculoBancoMock.Nome, "A propriedade 'Nome' do veículo de `VeiculoServicoMock` deveria ser igual a do veículo criado");

    var request = new HttpRequestMessage(HttpMethod.Delete, "/veiculos/1");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var response = await Setup.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "A requisição deveria retornar o status code '201 (No Content)'");

    veiculoBancoMock = veiculoServicoMock.BuscaPorId(1);

    Assert.IsNull(veiculoBancoMock, "O veículo de id 1 deveria ser apagado de `VeiculoServicoMock`");
  }
}