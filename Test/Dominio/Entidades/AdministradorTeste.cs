using MinimalApi.Dominio.Entidades;
namespace Test.Dominio.Entidades;

// Indica que esta classe contém métodos de teste unitário
[TestClass]
// Classe `sealed` é uma classe que não pode ser herdada
public sealed class AdministradorTeste
{
  // Indica que este método será executado como um teste unitário
  [TestMethod]
  // Testa se as propriedades de `Administrador` podem ser atribuídas e lidas corretamente.
  public void TestarGetSetPropriedades()
  {
    // Arrange - onde é definido o objeto a ser testado
    var adm = new Administrador();

    // Act - onde é feita as ações com o objeto que vão ser testadas
    // Testa o "Set" de `adm`, verificando se ele recebe os dados passados
    adm.Id = 1;
    adm.Email = "teste@teste.com";
    adm.Senha = "teste123";
    adm.Perfil = "Adm";

    // Assert - onde é testado se o resultado das ações feitas no objeto tem uma saída esperada
    // Testa o "Get" de `adm`, verificando se os dados passados foram "escritos" na propridade
    Assert.AreEqual(1, adm.Id);
    Assert.AreEqual("teste@teste.com", adm.Email);
    Assert.AreEqual("teste123", adm.Senha);
    Assert.AreEqual("Adm", adm.Perfil);
  }
}
