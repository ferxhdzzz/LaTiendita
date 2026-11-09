
using LaTiendita.Domain.Entities;

namespace LaTiendita.Domain.Interfaces
{
    public interface IVentaRepository : IRepository<Venta>
    {
        Task<Venta?> GetVentaConDetallesAsync(int id);
        Task<IEnumerable<Venta>> GetVentasConDetallesAsync();
        Task<IEnumerable<Venta>> GetVentasPorRangoFechaAsync(DateTime inicio, DateTime fin);
    }
}