using System.Net;
using System.Text;
using System.Text.Json;
using MinimalApi.Dominio.DTOs;
using Test.Helpers;

namespace Test.Requests;

[TestClass]
public sealed class AdministradorRequestTeste
{
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

  [TestMethod]
  public async Task TestarGetSetPropriedades()
  {
    // Variável do tipo `loginDTO` com as credenciais de login do administrador padrão do mock `AdministradorServicoMock`
    LoginDTO loginDTO = new LoginDTO
    {
      Email = "adm@teste.com",
      Senha = "123456"
    };

    // Armazena o conteúdo de `loginDTO` serializado em JSON para enviar no corpo da requisição
    var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

    // Fazendo a requisição na rota "/administradores/login" enviando `content` no corpo da requição, e armazena a resposta
    var response = await Setup.client.PostAsync("/administradores/login", content);

    // Verifica se a requisição teve um código de status `OK`
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
  }
}