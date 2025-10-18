using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MinimalApi.Dominio.Interfaces;
using Test.Mocks;
using Microsoft.Extensions.DependencyInjection;
using MinimalApi.Utils;

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
  // Instância estática de `JwtUtils` para gerenciar operações relacionadas a JWT
  public static JwtUtils JwtUtils = new JwtUtils();

  public static void ClassInit(TestContext testContext)
  {
    // Atribui o valor de `Setup.testContext` para o `testContext` do teste atual
    Setup.testContext = testContext;
    // Sobe um servidor em memória para testes
    Setup.http = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
    {
      // Define a porta do localhost que será usada, e define o ambiente da aplicação como "Test"
      builder.UseSetting("https_port", Setup.PORT).UseEnvironment("Test");

      builder.ConfigureAppConfiguration(config =>
      {
        var buildConfig = config.Build();

        // Define a chave JWT de teste na propriedade estática `JwtKey`
        var jwtKey = buildConfig.GetSection("Jwt")["Key"] ?? "";
        JwtUtils.JwtKey = jwtKey;
      });

      // Cria um cliente http com as configurações de `Setup.http`
      builder.ConfigureServices(services =>
      {
        // Injeta a dependência de `AdministradorServicoMock` ao chamar `IAdministradorServico`
        services.AddScoped<IAdministradorServico, AdministradorServicoMock>();
        // Injeta a dependência de `IVeiculoServico` ao chamar `VeiculoServicoMock`
        services.AddScoped<IVeiculoServico, VeiculoServicoMock>();
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