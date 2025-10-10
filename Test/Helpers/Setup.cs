using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MinimalApi.Dominio.Interfaces;
using Test.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Helpers;
// Classe que vai fazer requests em memória para os testes
public class Setup
{
  // Porta http que vai ser aberta em memória
  public const string PORT = "5080";
  // `TestContext` guarda os dados do teste atual, a declaração `static` diz que só vai haver um `testContext` para ser usado em toda a classe
  public static TestContext testContext = default!;
  // `WebApplicationFactory` permite subir a aplicação em memória simulando um servidor web, e usa os métodos `Configure` e `ConfigureServices` da classe `Startup`
  public static WebApplicationFactory<Startup> http = default!;
  // `HttpClient` permite fazer requisições http
  public static HttpClient client = default!;

  public static void ClassInit(TestContext testContext)
  {
    // Atribui o valor de `Setup.testContext` para o `testContext` do teste atual
    Setup.testContext = testContext;
    // Sobe um servidor em memória para testes
    Setup.http = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
    {
      // Define a porta do localhost que será usada, e define o ambiente da aplicação como "Testing"
      builder.UseSetting("https_port", Setup.PORT).UseEnvironment("Testing");

      builder.ConfigureServices(services =>
      {
        // Injeta a dependência de `AdministradorServicoMock` ao chamar `IAdministradorServico`
        services.AddScoped<IAdministradorServico, AdministradorServicoMock>();
      });
    });

    Setup.client = Setup.http.CreateClient(new WebApplicationFactoryClientOptions
    {
      BaseAddress = new Uri($"http://localhost:{Setup.PORT}"),
    });
  }

  public static void ClassCleanup()
  {
    Setup.http?.Dispose();
  }
}