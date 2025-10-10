using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servico;

public class AdministradorServico : IAdministradorServico
{
  private readonly DbContexto _contexto;
  public AdministradorServico(DbContexto dbContexto)
  {
    _contexto = dbContexto;
  }

  public Administrador? BuscaPorId(int id)
  {
    Administrador? administrador = _contexto.Administradores.Where(item => item.Id == id).FirstOrDefault();

    return administrador;
  }

  public Administrador Incluir(Administrador administrador)
  {
    _contexto.Administradores.Add(administrador);
    _contexto.SaveChanges();

    return administrador;
  }

  public Administrador? Login(LoginDTO loginDTO)
  {
    Administrador? adm = _contexto.Administradores.Where(item => item.Email == loginDTO.Email && item.Senha == loginDTO.Senha).FirstOrDefault();

    return adm;
  }

  public List<Administrador> Todos(int? pagina = 1)
  {
    // A `query` só vai terminar quando for chamado algum método de execução (`ToList()`, `ToString()`, etc)
    var query = _contexto.Administradores.AsQueryable();

    int itensPorPagina = 10;

    if (pagina != null)
    {
      pagina = 1;
      query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
    }

    // Terminando a consulta e retornando o resultado dela
    return query.ToList();
  }
}