
using LaTiendita.Domain.Entities;

namespace LaTiendita.Domain.Interfaces
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetProductosConCategoriaAsync();
        Task<IEnumerable<Producto>> GetProductosStockBajoAsync();
    }
}