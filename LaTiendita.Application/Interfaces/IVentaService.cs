using LaTiendita.Application.DTOs;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IVentaService
    {
        Task<IEnumerable<Venta>> GetAllAsync();
        Task<Venta?> GetByIdAsync(int id);
        Task<(bool Exito, string Mensaje)> RegistrarVentaAsync(VentaRegistroDto dto);
    }
}