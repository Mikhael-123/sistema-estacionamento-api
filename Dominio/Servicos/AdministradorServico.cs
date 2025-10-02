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

  public Administrador? Login(LoginDTO loginDTO)
  {
    Administrador? listAdm = _contexto.Administradores.Where(item => item.Email == "adm@teste.com" && item.Senha == "123456").FirstOrDefault();

    return listAdm; 
  }
}