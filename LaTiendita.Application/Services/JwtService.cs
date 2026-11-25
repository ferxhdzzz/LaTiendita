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

        public int DurationInHours => _durationInHours;

        public JwtService(
            string secretKey,
            string issuer = "LaTienditaApi",
            string audience = "LaTienditaClients",
            int durationInHours = 8)
        {
            if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
                throw new ArgumentException("La clave secreta JWT es obligatoria y debe tener al menos 32 bytes.", nameof(secretKey));

            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
            _durationInHours = durationInHours > 0 ? durationInHours : 8;
        }

        public string GenerarToken(Empleado empleado)
        {
            return GenerarToken(
                empleado.Id.ToString(),
                empleado.Nombre,
                empleado.Correo,
                new[] { empleado.Rol.ToString() },
                empleado.Id);
        }

        public string GenerarToken(string userId, string nombre, string correo, IEnumerable<string> roles, int? empleadoId = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Email, correo),
                new Claim("UsuarioId", userId)
            };

            if (empleadoId.HasValue)
            {
                claims.Add(new Claim("EmpleadoId", empleadoId.Value.ToString()));
            }

            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

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