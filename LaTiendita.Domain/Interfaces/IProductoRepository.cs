
using LaTiendita.Domain.Entities;

namespace LaTiendita.Domain.Interfaces
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetProductosConCategoriaAsync();
        Task<Producto?> GetByIdConCategoriaAsync(int id);
        Task<IEnumerable<Producto>> GetProductosStockBajoAsync();
    }
}