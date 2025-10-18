using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;

namespace Test.Mocks;

public class VeiculoServicoMock : IVeiculoServico
{
  private static List<Veiculo> veiculos = new List<Veiculo>();

  public void ApagarTudo()
  {
    veiculos.Clear();

    return;
  }
  public void Apagar(Veiculo veiculo)
  {
    veiculos.Remove(veiculo);

    return;
  }

  public void Atualizar(Veiculo veiculo)
  {
    Veiculo? veiculoLista = veiculos.Find(item => item.Id == veiculo.Id);

    if (veiculoLista == null) return;

    veiculoLista.Nome = veiculo.Nome;
    veiculoLista.Marca = veiculo.Marca;
    veiculoLista.Ano = veiculo.Ano;

    return;
  }

  public Veiculo? BuscaPorId(int id)
  {
    Veiculo? veiculo = veiculos.Find(item => item.Id == id);

    return veiculo;
  }

  public void Incluir(Veiculo veiculo)
  {
    // "auto-incrementa" o id
    veiculo.Id = veiculos.Count() + 1;
    veiculos.Add(veiculo);

    return;
  }

  public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
  {
    int itensPorPagina = 10;

    if (pagina != null)
    {
      return veiculos.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina).ToList();
    }

    return veiculos;
  }
}