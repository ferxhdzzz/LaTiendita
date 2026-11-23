using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _durationInHours;

        public JwtService(
            string secretKey = "LaTiendita_SuperSecretKey_2026_DesarrolloWebSeguro_TokenKeyJWT!",
            string issuer = "LaTienditaApi",
            string audience = "LaTienditaClients",
            int durationInHours = 8)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _durationInHours = durationInHours;
        }

        public string GenerarToken(Empleado empleado)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims: información que viaja protegida dentro del Token JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, empleado.Id.ToString()),
                new Claim(ClaimTypes.Name, empleado.Nombre),
                new Claim(ClaimTypes.Email, empleado.Correo),
                new Claim(ClaimTypes.Role, empleado.Rol.ToString()),
                new Claim("UsuarioId", empleado.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_durationInHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}