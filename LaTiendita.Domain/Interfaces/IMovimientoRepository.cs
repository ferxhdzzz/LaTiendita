using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Enums;

namespace LaTiendita.Domain.Interfaces
{
    public interface IMovimientoRepository : IRepository<Movimiento>
    {
        Task<IEnumerable<Movimiento>> GetMovimientosConDetallesAsync(TipoMovimiento? tipo = null, string? buscar = null);
    }
}
