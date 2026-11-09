using Microsoft.EntityFrameworkCore.Storage;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositorios específicos
        public IProductoRepository Productos { get; }
        public IVentaRepository Ventas { get; }

        // Repositorios genéricos
        public IRepository<Empleado> Empleados { get; }
        public IRepository<Categoria> Categorias { get; }
        public IRepository<Proveedor> Proveedores { get; }
        public IRepository<Compra> Compras { get; }
        public IRepository<Movimiento> Movimientos { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Productos = new ProductoRepository(_context);
            Ventas = new VentaRepository(_context);
            Empleados = new Repository<Empleado>(_context);
            Categorias = new Repository<Categoria>(_context);
            Proveedores = new Repository<Proveedor>(_context);
            Compras = new Repository<Compra>(_context);
            Movimientos = new Repository<Movimiento>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}