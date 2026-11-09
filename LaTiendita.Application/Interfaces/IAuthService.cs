using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Empleado?> LoginAsync(string correo, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}