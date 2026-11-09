namespace LaTiendita.Domain.Entities
{
    public class Compra
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }

        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        // Empleado/Admin que registró la compra
        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        // Lista de productos comprados
        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}