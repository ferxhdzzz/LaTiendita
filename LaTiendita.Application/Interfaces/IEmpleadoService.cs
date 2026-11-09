using LaTiendita.Application.DTOs;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IEmpleadoService
    {
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado?> GetByIdAsync(int id);
        Task<Empleado?> AutenticarAsync(string correo, string password);
        Task<bool> CrearAsync(EmpleadoDto dto);
        Task<bool> ActualizarAsync(EmpleadoDto dto);
        Task<bool> EliminarAsync(int id);
    }
}