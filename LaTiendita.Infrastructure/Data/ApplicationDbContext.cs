using Microsoft.EntityFrameworkCore;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas del sistema
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precisiones para valores monetarios (Evita advertencias de SQL Server)
            modelBuilder.Entity<Producto>().Property(p => p.Costo).HasPrecision(18, 2);
            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);

            modelBuilder.Entity<Venta>().Property(v => v.Total).HasPrecision(18, 2);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);
            modelBuilder.Entity<DetalleVenta>().Property(d => d.CostoUnitario).HasPrecision(18, 2);

            modelBuilder.Entity<Compra>().Property(c => c.Total).HasPrecision(18, 2);
            modelBuilder.Entity<DetalleCompra>().Property(d => d.CostoUnitario).HasPrecision(18, 2);

            // Relación Venta -> Detalles (Eliminación en cascada)
            modelBuilder.Entity<Venta>()
                .HasMany(v => v.Detalles)
                .WithOne(d => d.Venta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Compra -> Detalles (Eliminación en cascada)
            modelBuilder.Entity<Compra>()
                .HasMany(c => c.Detalles)
                .WithOne(d => d.Compra)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}