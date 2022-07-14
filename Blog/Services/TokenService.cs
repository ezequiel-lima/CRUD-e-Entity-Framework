using Blog.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace Blog.Services
{
    // Finalidade dessa classe é gerar um token
    public class TokenService
    {
        // Necessario package Microsoft.AspNetCore.Authentication
        // Necessario package Microsoft.AspNetCore.Authentication.Bearer
        public string GenerateToken(User user)
        {
            // Criamos a instancia do tokenHandler que é o item para gerar o nosso token
            var tokenHandler = new JwtSecurityTokenHandler();
            // Pegamos a chave
            var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);
            // Criamos a especificação desse token 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Informações sobre esse token
                Subject = new ClaimsIdentity(new Claim[]
                {
                    //chave e valor
                    new Claim(ClaimTypes.Name, value:"quiel"), // User.Identity.Name
                    new Claim(ClaimTypes.Role, value:"user"),
                    new Claim(ClaimTypes.Role, value:"admin"), // User.IsInRole
                    new Claim("", value:"")
                }),
                // Define um tempo de duração do token
                Expires = DateTime.UtcNow.AddHours(8),
                // Como vai ser gerado e lido depois
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };
            // Depois criamos o token 
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
