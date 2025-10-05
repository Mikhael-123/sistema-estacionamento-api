using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servico;

public class VeiculoServico : IVeiculoServico
{
  private readonly DbContexto _contexto;
  public VeiculoServico(DbContexto dbContexto)
  {
    _contexto = dbContexto;
  }

  public void Apagar(Veiculo veiculo)
  {
    _contexto.Veiculos.Remove(veiculo);
    _contexto.SaveChanges();
  }

  public void Atualizar(Veiculo veiculo)
  {
    _contexto.Veiculos.Update(veiculo);
    _contexto.SaveChanges();
  }

  public Veiculo? BuscaPorId(int id)
  {
    Veiculo? veiculo = _contexto.Veiculos.Where(item => item.Id == id).FirstOrDefault();
    return veiculo;
  }

  public void Incluir(Veiculo veiculo)
  {
    _contexto.Veiculos.Add(veiculo);
    _contexto.SaveChanges();
  }

  public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null)
  {
    // `AsQueryable()` permite ir "montando" a `query` para ir filtrando os dados de `Veiculos`
    var query = _contexto.Veiculos.AsQueryable();

    // Verifica se `nome` não é nulo
    if (!string.IsNullOrEmpty(nome))
    {
      // Reatribui a `query` com os itens filtrados por `nome`
      query = query.Where(item => item.Nome.ToLower().Contains(nome.ToLower()));
    }

    // Verifica se `marca` não é nulo
    if (!string.IsNullOrEmpty(marca))
    {
      // Reatribui a `query` com os itens filtrados por `marca`
      query = query.Where(item => item.Marca.ToLower().Contains(marca.ToLower()));
    }

    // Define quantos itens serão pegos por página
    int itensPorPagina = 10;

    // "Pula" a quantidade de `itensPorPagina` * (pagina - 1) e "Pega" a quantidade de `itensPorPagina` dos itens da `query`, e reatribui o valor
    if (pagina != null)
      query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

    // Retorna uma lista da `query` feita, contendo os veiculos filtrados
      return query.ToList();
  }
}