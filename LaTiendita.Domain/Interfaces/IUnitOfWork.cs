
using LaTiendita.Domain.Entities;

namespace LaTiendita.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductoRepository Productos { get; }
        IVentaRepository Ventas { get; }
        IRepository<Empleado> Empleados { get; }
        IRepository<Categoria> Categorias { get; }
        IRepository<Proveedor> Proveedores { get; }
        IRepository<Compra> Compras { get; }
        IRepository<Movimiento> Movimientos { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}