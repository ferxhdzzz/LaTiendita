using Microsoft.EntityFrameworkCore;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Infrastructure.Repositories
{
    public class CompraRepository : Repository<Compra>, ICompraRepository
    {
        public CompraRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Compra?> GetCompraConDetallesAsync(int id)
        {
            return await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Compra>> GetComprasConDetallesAsync()
        {
            return await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }
    }
}
