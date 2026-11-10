using LaTiendita.Domain.Entities;

namespace LaTiendita.Domain.Interfaces
{
    public interface ICompraRepository : IRepository<Compra>
    {
        Task<Compra?> GetCompraConDetallesAsync(int id);
        Task<IEnumerable<Compra>> GetComprasConDetallesAsync();
    }
}
