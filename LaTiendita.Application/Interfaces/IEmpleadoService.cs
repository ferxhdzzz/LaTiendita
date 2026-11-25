using LaTiendita.Application.DTOs;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IEmpleadoService
    {
        Task<IEnumerable<Empleado>> GetAllAsync();
        Task<Empleado?> GetByIdAsync(int id);
        Task<Empleado?> CrearAsync(EmpleadoDto dto);
        Task<bool> ActualizarAsync(EmpleadoDto dto);
        Task<bool> EliminarAsync(int id);
    }
}