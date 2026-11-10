using Microsoft.EntityFrameworkCore;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Enums;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Infrastructure.Repositories
{
    public class MovimientoRepository : Repository<Movimiento>, IMovimientoRepository
    {
        public MovimientoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Movimiento>> GetMovimientosConDetallesAsync(TipoMovimiento? tipo = null, string? buscar = null)
        {
            var query = _context.Movimientos
                .Include(m => m.Producto)
                .Include(m => m.Empleado)
                .AsQueryable();

            if (tipo.HasValue)
            {
                query = query.Where(m => m.Tipo == tipo.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(m => m.Producto != null && m.Producto.Nombre.Contains(buscar));
            }

            return await query
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }
    }
}
