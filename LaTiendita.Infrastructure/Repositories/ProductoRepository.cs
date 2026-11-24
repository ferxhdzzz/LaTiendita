using Microsoft.EntityFrameworkCore;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Infrastructure.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Producto>> GetProductosConCategoriaAsync()
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Activo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetProductosStockBajoAsync()
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Activo && p.Stock <= p.StockMinimo)
                .ToListAsync();
        }
    }
}