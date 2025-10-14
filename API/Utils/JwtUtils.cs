using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Utils;

public class JwtUtils
{
  public string JwtKey = default!;

  public string GerarTokenJWT(Administrador? administrador)
  {
    // Se o administrador for nulo retorna uma string vazia
    if (administrador == null) return "";

    // `securityKey` guarda a criptografia da chave JWT
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
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
};