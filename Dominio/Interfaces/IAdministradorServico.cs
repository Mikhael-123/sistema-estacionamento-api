using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorServico
{
  Administrador? Login(LoginDTO loginDTO);
  Administrador Incluir(Administrador administrador);
  List<Administrador> Todos(int? pagina = 1);
  Administrador? BuscaPorId(int id);
}