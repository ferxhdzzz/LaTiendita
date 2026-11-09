namespace LaTiendita.Domain.Entities
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }

        // Empleado que realizó la venta
        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        // Lista de productos vendidos
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}