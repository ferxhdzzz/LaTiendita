using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IJwtService
    {
        int DurationInHours { get; }
        string GenerarToken(Empleado empleado);
        string GenerarToken(string userId, string nombre, string correo, IEnumerable<string> roles, int? empleadoId = null);
    }
}