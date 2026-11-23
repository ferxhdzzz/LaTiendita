using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerarToken(Empleado empleado);
    }
}