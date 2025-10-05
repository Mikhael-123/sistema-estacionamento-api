using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servico;
using MinimalApi.Infraestrutura.Db;

public class Startup
{
  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;
    // Pega a chave JWT do arquivo "appsettings.json", ou uma string vazia se não for possível acessar a chave
    jwtKey = Configuration.GetSection("Jwt")["Key"] ?? "";
  }

  private string jwtKey = default!;
  public IConfiguration Configuration { get; set; } = default!;

  public void ConfigureServices(IServiceCollection services)
  {
    // Adicionando swagger
    services.AddEndpointsApiExplorer();
    // Adiciona e configura o Swagger na aplicação
    services.AddSwaggerGen(options =>
    {
      // --- Define um esquema de segurança chamado "Bearer" ---
      // Isso informa ao Swagger que o app usa autenticação via token JWT no header Authorization.
      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Name = "Authorization",              // Nome do header HTTP usado para enviar o token
        Type = SecuritySchemeType.Http,      // Tipo de autenticação HTTP (vs. API key, OAuth2 etc.)
        Scheme = "bearer",                   // Esquema do tipo "Bearer" (autenticação baseada em token)
        BearerFormat = "JWT",                // Indica que o formato do token é JWT
        In = ParameterLocation.Header,       // Diz que o token vai no cabeçalho HTTP
        Description = "Insira o token JWT recebido ao logar, para ter acesso aos endpoints"
        // Descrição exibida na interface do Swagger (ajuda o usuário)
      });

      // --- Define o requisito de segurança ---
      // Aqui estamos dizendo ao Swagger que, por padrão, todas as rotas exigem o esquema "Bearer"
      options.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecurityScheme{
            Reference = new OpenApiReference{
              Type = ReferenceType.SecurityScheme, // Diz que está se referindo a um esquema já definido
              Id = "Bearer",                       // Nome do esquema definido acima
            }
          },
          new string[] { } // Escopos (vazio porque não há escopos como em OAuth2)
        }
      });
    });

    // Adiciona o serviço de autorização de rotas
    services.AddAuthorization();

    // Adicionando o serviço de autenticação por tokens JWT
    services.AddAuthentication(option =>
    {
      option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(option =>
    {
      option.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
      };
    });

    // Diz que ao chamar `IAdministradorServico`, é para criar uma instância de `AdministradorServico`
    services.AddScoped<IAdministradorServico, AdministradorServico>();
    // Diz que ao chamar `IVeiculoServico`, é para criar uma instância de `VeiculoServico`
    services.AddScoped<IVeiculoServico, VeiculoServico>();

    // Configura o `builder` para conectar ao banco de dados
    services.AddDbContext<DbContexto>(opt =>
    {
      // Pega a propriedade "mysql" da "ConnectionStrings" do arquivo appsettings.json
      var stringConexao = Configuration.GetConnectionString("MySql");
      // Conecta ao banco de dados MySQL
      opt.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
    });
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseRouting();

    // Usa a autenticação e autorização de rotas respectivamente *trocar a ordem pode dar erro*
    app.UseAuthentication();
    // OBS: É necessário adicionar o serviço `AddAuthorization` no `builder` também
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
      # region Home
      // Rota padrão da aplicação, retorna um json com algumas informações
      endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous();
      # endregion

      # region Administradores
      // Loga o usuário como admnistrador se as credenciais estiverem corretas
      endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
      {
        Administrador? adm = administradorServico.Login(loginDTO);

        if (adm != null)
        {
          var token = GerarTokenJWT(adm);

          return Results.Ok(new AdministradorLogado
          {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token,
          });
        }
        else
          return Results.Unauthorized();
      }).WithTags("Administradores").AllowAnonymous();

      endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
      {
        ErrosDeValidacao validacao = validaDTO(administradorDTO);
        // Se a quantidade de strings dentro de `Mensagens` for maior que 0, é retornado um "BadRequest"
        if (validacao.Mensagens.Count > 0)
          return Results.BadRequest(validacao);

        Administrador administrador = new Administrador
        {
          Email = administradorDTO.Email,
          Senha = administradorDTO.Senha,
          Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(),
        };
        administradorServico.Incluir(administrador);

        var administradorView = new AdministradorModelView
        {
          Id = administrador.Id,
          Email = administrador.Email,
          Perfil = administrador.Perfil,
        };

        return Results.Created($"/administradores/{administradorView.Id}", administradorView);
      }).WithTags("Administradores").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

      endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
      {
        // `administradores` guarda todos os dados dos administradores do banco dados
        List<Administrador> administradores = administradorServico.Todos(pagina);
        // `administradoresView` guarda dados de administradores que podem ser mostrados ao usuário
        List<AdministradorModelView> administradoresView = PegarAdministradoresView(administradores);

        return Results.Ok(administradoresView);
      }).WithTags("Administradores").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

      endpoints.MapGet("/administradores/{id}", ([FromQuery] int id, IAdministradorServico administradorServico) =>
      {
        Administrador? administrador = administradorServico.BuscaPorId(id);

        if (administrador == null) return Results.NotFound();

        var administradorView = new AdministradorModelView
        {
          Id = administrador.Id,
          Email = administrador.Email,
          Perfil = administrador.Perfil,
        };

        return Results.Ok(administrador);
      }).WithTags("Administradores").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });
      # endregion

      # region Veiculos
      // Cadastra um novo veículo, pegando as informações pelo body da requisição
      endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
      {
        ErrosDeValidacao validacao = validaDTO(veiculoDTO);
        // Se a quantidade de strings dentro de `Mensagens` for maior que 0, é retornado um "BadRequest"
        if (validacao.Mensagens.Count > 0)
          return Results.BadRequest(validacao);

        Veiculo veiculo = new Veiculo
        {
          Nome = veiculoDTO.Nome,
          Marca = veiculoDTO.Marca,
          Ano = veiculoDTO.Ano,
        };
        veiculoServico.Incluir(veiculo);

        return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
      }).WithTags("Veiculos").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

      // Retorna alguns veículos cadastrados separados por página, que pode ser especificada na query
      endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
      {
        List<Veiculo> veiculos = veiculoServico.Todos(pagina);

        return Results.Ok(veiculos);
      }).WithTags("Veiculos").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

      // Retorna 1 veículo pelo Id especificado na rota
      endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
      {
        Veiculo? veiculo = veiculoServico.BuscaPorId(id);

        if (veiculo == null) return Results.NotFound();

        return Results.Ok(veiculo);
      }).WithTags("Veiculos").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" });

      // Atualiza um veículo pelo Id especificado na rota
      endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
      {
        ErrosDeValidacao validacao = validaDTO(veiculoDTO);
        // Se a quantidade de strings dentro de `Mensagens` for maior que 0, é retornado um "BadRequest"
        if (validacao.Mensagens.Count > 0)
          return Results.BadRequest(validacao);
        Veiculo? veiculo = veiculoServico.BuscaPorId(id);

        if (veiculo == null) return Results.NotFound();

        veiculo.Nome = veiculoDTO.Nome;
        veiculo.Marca = veiculoDTO.Marca;
        veiculo.Ano = veiculoDTO.Ano;

        veiculoServico.Atualizar(veiculo);

        return Results.NoContent();
      }).WithTags("Veiculos").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });

      // Deleta um veículo pelo Id especificado na rota
      endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
      {
        Veiculo? veiculo = veiculoServico.BuscaPorId(id);

        if (veiculo == null) return Results.NotFound();

        veiculoServico.Apagar(veiculo);

        return Results.NoContent();
      }).WithTags("Veiculos").RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" });
      # endregion
    });
  }

  // Função genérica que verifica a instância do DTO recebido e valida os dados
  private ErrosDeValidacao validaDTO<T>(T dto)
  {
    // Instanciando a classe `ErrosDeValidacao` para acumular as mensagens de erro de validação
    // A propriedade `Mensagens`, deve ser instanciada
    var validacao = new ErrosDeValidacao()
    {
      Mensagens = new List<string>()
    };

    // Verificando de qual instância o DTO recebido pertence, e validando as propriedades do DTO recebido
    if (dto is VeiculoDTO veiculoDTO)
    {
      if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O campo 'nome' não pode ficar em branco");

      if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("O campo 'marca' não pode ficar em branco");

      if (veiculoDTO.Ano < 1950)
        validacao.Mensagens.Add("O campo 'ano' não pode ser inferior à 1950");
    }
    else if (dto is AdministradorDTO administradorDTO)
    {
      if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("O campo 'email' não pode ficar em branco");
      if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("O campo 'senha' não pode ficar em branco");
      if (string.IsNullOrEmpty(administradorDTO.Perfil.ToString()))
        validacao.Mensagens.Add("O campo 'perfil' não pode ficar em branco");
    }
      
    return validacao;
  }

  // Recebe uma lista com todos os dados dos administradores do banco de dados e retorna uma lista com somente as informações permitidas
  private List<AdministradorModelView> PegarAdministradoresView(List<Administrador> administradoresModel)
  {
    // O parâmetro `administradoresModel` é uma lista de instâncias de `Administrador`, que possuem todos os dados relacionados aos administradores do banco de dados, `AdministradorModelView` é um objeto para mostrar apenas as informações permitidas ao usuário
    List<AdministradorModelView> administradoresView = new List<AdministradorModelView>();

    // Pegando os dados de `administradoresModel` e colocando em `administradoresView` para que sejam mostradas apenas as informações permitidas ao usuário
    foreach (Administrador administrador in administradoresModel)
    {
      AdministradorModelView administradorView = new AdministradorModelView
      {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil,
      };

      administradoresView.Add(administradorView);
    }

    // Retorna uma lista de `AdministradorModelView`, que mostram apenas as informações de administradores permitidas
    return administradoresView;
  }

  // Gera e retorna um Json Web Token
  private string GerarTokenJWT(Administrador administrador)
  {
    // `securityKey` guarda a criptografia da chave JWT
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    // Gera uma credencial passando a chave criptografada e o algoritmo de descriptografia, e guarda em `credentials`
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    // Cria uma lista de `Claim`, cada `Claim` é um dado do Json Web Token
    var claims = new List<Claim>
    {
      new Claim("Email", administrador.Email),
      new Claim(ClaimTypes.Role, administrador.Perfil),
    };

    // Cria um token com a lista de `Claim`, com duração de 1 dia e com as credenciais geradas
    var token = new JwtSecurityToken(
      claims: claims,
      expires: DateTime.Now.AddDays(1),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}