using LaTiendita.Application.DTOs;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IProductoService
    {
        Task<IEnumerable<Producto>> GetAllAsync();
        Task<IEnumerable<Producto>> GetStockBajoAsync();
        Task<Producto?> GetByIdAsync(int id);
        Task<Producto?> CrearAsync(ProductoDto dto);
        Task<Producto?> ActualizarAsync(ProductoDto dto);
        Task<bool> EliminarAsync(int id);
    }
}