using System.Security.Cryptography;
using System.Text;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Principio DIP: Se inyecta la abstracción IUnitOfWork
        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Empleado?> LoginAsync(string correo, string password)
        {
            var empleados = await _unitOfWork.Empleados.FindAsync(e => e.Correo.ToLower() == correo.ToLower() && e.Activo);
            var empleado = empleados.FirstOrDefault();

            if (empleado == null) return null;

            // Verificar la contraseña cifrada
            if (!VerifyPassword(password, empleado.PasswordHash)) return null;

            return empleado;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashEntrada = HashPassword(password);
            return hashEntrada == hash;
        }
    }
}