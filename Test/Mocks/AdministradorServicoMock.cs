using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;

namespace Test.Mocks;

// Classe que vai "mockar" os dados da base de dados "administradores", ou seja, simular dados que seriam retornados da base de dados, para os testes manipularem dados simulados ao invés dos dados reais
public class AdministradorServicoMock : IAdministradorServico
{
  // Lista que vai simular o banco de dados, com um administrador padrão
  private static List<Administrador> administradores = new List<Administrador>
  {
    new Administrador{
      Id = 1,
      Email = "adm@teste.com",
      Senha = "123456",
      Perfil = "Adm"
    },
    new Administrador{
      Id = 2,
      Email = "editor@teste.com",
      Senha = "123456",
      Perfil = "Editor"
    },
  }; 

  public Administrador? BuscaPorId(int id)
  {
    Administrador? administrador = administradores.Find(item => item.Id == id);

    return administrador;
  }

  public Administrador Incluir(Administrador administrador)
  {
    // "auto-incrementa" o id
    administrador.Id = administradores.Count() + 1;
    // Adiciona o administrador recebido na lista
    administradores.Add(administrador);

    return administrador;
  }

  public Administrador? Login(LoginDTO loginDTO)
  {
    Administrador? administrador = administradores.Find(item => item.Senha == loginDTO.Senha && item.Senha == loginDTO.Senha);

    return administrador;
  }

  public List<Administrador> Todos(int? pagina = 1)
  {
    return administradores;
  }
}