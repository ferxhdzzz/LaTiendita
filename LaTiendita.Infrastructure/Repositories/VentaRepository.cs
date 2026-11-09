using Microsoft.EntityFrameworkCore;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Infrastructure.Repositories
{
    public class VentaRepository : Repository<Venta>, IVentaRepository
    {
        public VentaRepository(ApplicationDbContext context) : base(context)
        {
        }

        // LINQ: Venta con Detalles, Productos y Empleado
        public async Task<Venta?> GetVentaConDetallesAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Empleado)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        // LINQ: Listar todas las ventas ordenadas por fecha más reciente
        public async Task<IEnumerable<Venta>> GetVentasConDetallesAsync()
        {
            return await _context.Ventas
                .Include(v => v.Empleado)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }

        // LINQ: Filtro por rango de fechas (para Reportes de ventas)
        public async Task<IEnumerable<Venta>> GetVentasPorRangoFechaAsync(DateTime inicio, DateTime fin)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }
    }
}