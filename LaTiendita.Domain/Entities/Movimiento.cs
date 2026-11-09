using LaTiendita.Domain.Enums;

namespace LaTiendita.Domain.Entities
{
    public class Movimiento
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public TipoMovimiento Tipo { get; set; } // Entrada, Salida, Ajuste
        public int Cantidad { get; set; }
        public string Motivo { get; set; } = string.Empty; // Ej: "Venta #3", "Compra a Distribuidora"

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }
    }
}