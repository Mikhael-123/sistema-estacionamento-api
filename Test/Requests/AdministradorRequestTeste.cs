using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.ModelViews;
using Test.Helpers;
using Test.Mocks;

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

  private const string category = "AdministradorRequest";

  [TestMethod]
  [TestCategory($"{category}")]
  [Description("Verifica o administrador padrão retornado de `AdministradorServicoMock`")]
  public void TestandoAdministradorPadrao()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);

    Assert.IsNotNull(admPadrao, "Deveria ser retornado um administrador padrão de `AdministradorServicoMock`");
    // Verifica se o administrador retornado tem perfil de administrador
    Assert.AreEqual(Perfil.Adm.ToString(), admPadrao.Perfil, "O administrador padrão deveria ter o perfil de administrador");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se a rota 'POST:/administradores/login' recebe as credenciais de um administrador e retorna um token JWT")]
  public async Task TestandoPostLogin()
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

    // Verifica se as credenciais de `loginDTO` é igual as credenciais do administrador padrão de `AdministradorServicoMock`
    Assert.AreEqual("adm@teste.com", loginDTO.Email, "A credencial de login 'Email' deve ser igual a propriedade 'Email' do administrador padrão");
    Assert.AreEqual("123456", loginDTO.Senha, "A credencial de login 'Senha' deve ser igual a propriedade 'Senha' do administrador padrão");
    // Verifica se a requisição teve um código de status `OK`
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "A requisição deve retornar o status http '200 (OK)'");

    // Armazena o conteúdo json retornado da requisição
    var result = await response.Content.ReadAsStringAsync();
    // "Deserializa" o json de `result` como um `AdministradorLogado`
    var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
    });

    // Verifica os dados retornados da requisição
    Assert.IsNotNull(admLogado, "Deveria ser retornado o administrador padrão do banco de dados");
    Assert.IsNotNull(admLogado.Email, "A propriedade retornada 'Email' deveria ser igual a propriedade 'Email' do administrador padrão");
    Assert.IsNotNull(admLogado.Perfil, "A propriedade retornada 'Perfil' deveria ser igual a propriedade 'Perfil' do administrador padrão");
    Assert.IsNotNull(admLogado.Token, "Deveria ser retornado um token JWT ao usuário");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se a rota 'POST:/administradores' cria um administrador, e retorna o administrador criado e a rota para acessar o administrador")]
  public async Task TestandoPostAdministradores()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    // Pega o administrador padrão do mock do servico de administrador
    var admPadrao = administradorServicoMock.BuscaPorId(1);

    // Cria um token JWT para o administrador padrão
    var token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    // Cria uma instância de `AdministradorDTO`
    AdministradorDTO administradorDTO = new AdministradorDTO
    {
      Email = "novoAdministrador@teste.com",
      Senha = "novaSenha123456",
      Perfil = Perfil.Adm,
    };
    // Cria uma instância de `HttpRequestMessage` para configurar a requisição que vai ser feita
    var request = new HttpRequestMessage(HttpMethod.Post, "/administradores");
    // Coloca o token JWT em Authorization no Header da requisição
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    // Transforma `administradorDTO` em um json "serializado"
    var content = new StringContent(JsonSerializer.Serialize(administradorDTO), Encoding.UTF8, "application/json");
    // Coloca `content` no conteúdo da requisição
    request.Content = content;

    // Faz uma requisição com as configurações de `request`
    var response = await Setup.client.SendAsync(request);

    // Verifica se a requisição retornou um status http "201 (Created)"
    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "A requisição deveria retornar o status http '201 (Created)'");

    // Guarda os dados retornados da requisição
    var result = await response.Content.ReadAsStringAsync();
    // "Deserializa" o json `result` e assimila a uma instância de `AdministradorModelView`
    var admResult = JsonSerializer.Deserialize<AdministradorModelView>(result, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
    });

    // Verifica se a requisição retorna uma instância de `AdministradorModelView`
    Assert.IsNotNull(admResult, "A requisição deveria retornar uma model view de admininistrador");
    // Verifica se a requisição retorna o caminho para acessar o administrador criado
    Assert.AreEqual("/administradores/3", Convert.ToString(response.Headers.Location));
    // Verifica o id do administrador retornado, já existem dois registros no "banco", o administrador criado é o terceiro registro
    Assert.AreEqual(3, admResult.Id, "O administrador retornado deveria ter o id 3");
    // Verifica os restos das propriedades do administrador retornado
    Assert.AreEqual("novoAdministrador@teste.com", admResult.Email, "A propriedade 'Email' do administrador retornado deve ser igual a do administrador criado");
    Assert.AreEqual(Perfil.Adm.ToString(), admResult.Perfil, "A propriedade 'Perfil' do administrador retornado deve ser igual a do administrador criado");
  }

  [TestMethod]
  [TestCategory(category)]
  [Description("Verifica se a rota 'GET:/administradores' retorna uma lista de administradores do tipo `AdministradoresModelView`")]
  public async Task TestandoGetAdministradores()
  {
    var administradorServicoMock = new AdministradorServicoMock();
    var admPadrao = administradorServicoMock.BuscaPorId(1);

    Assert.IsNotNull(admPadrao, "Deveria ser retornado um administrador padrão");
    Assert.AreEqual(Perfil.Adm.ToString(), admPadrao.Perfil);

    var token = Setup.JwtUtils.GerarTokenJWT(admPadrao);

    var request = new HttpRequestMessage(HttpMethod.Get, "/administradores");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var response = await Setup.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "A requisição deveria retorna o status http '200 (OK)'");

    var content = await response.Content.ReadAsStringAsync();
    var administradoresView = JsonSerializer.Deserialize<List<AdministradorModelView>>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.IsNotNull(administradoresView, "A requisição deveria retornar uma lista de administradores do tipo `AdministradoresModelView`");
    Assert.AreEqual(2, administradoresView.Count(), "A lista de administradores deveria conter um administrador e editor padrão");

    request = new HttpRequestMessage(HttpMethod.Get, "/administradores?pagina=2");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    response = await Setup.client.SendAsync(request);

    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "A requisição deveria retorna o status http '200 (OK)'");

    content = await response.Content.ReadAsStringAsync();
    administradoresView = JsonSerializer.Deserialize<List<AdministradorModelView>>(content, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true,
    });

    Assert.IsNotNull(administradoresView, "A requisição deveria retornar uma lista de administradores do tipo `AdministradoresModelView`");
    Assert.AreEqual(0, administradoresView.Count(), "A lista de administradores deveria estar vazia pois foi pedida a página 2");
  }
}