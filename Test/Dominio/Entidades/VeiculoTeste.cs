using MinimalApi.Dominio.Entidades;

namespace Test.Dominio.Entidades;

[TestClass]
public sealed class VeiculoTeste
{
  [TestMethod]
  public void TestarGetSetPropriedades()
  {
    // Arrange
    var veiculo = new Veiculo();

    // Act
    veiculo.Id = 1;
    veiculo.Nome = "VeiculoTeste";
    veiculo.Marca = "MarcaTeste";
    veiculo.Ano = 1890;

    // Assert
    Assert.AreEqual(1, veiculo.Id);
    Assert.AreEqual("VeiculoTeste", veiculo.Nome);
    Assert.AreEqual("MarcaTeste", veiculo.Marca);
    Assert.AreEqual(1890, veiculo.Ano);
  }
}