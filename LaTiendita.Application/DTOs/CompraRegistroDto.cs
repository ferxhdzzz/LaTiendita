using System.ComponentModel.DataAnnotations;

namespace LaTiendita.Application.DTOs
{
    public class CompraItemDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador de producto no es válido.")]
        public int ProductoId { get; set; }

        [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [Range(0.01, 100000.00, ErrorMessage = "El costo unitario debe ser mayor a cero.")]
        public decimal CostoUnitario { get; set; }

        public DateTime? FechaVencimiento { get; set; }
    }

    public class CompraRegistroDto
    {
        public int? ProveedorId { get; set; }
        public int? EmpleadoId { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un producto en la compra.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la compra.")]
        public List<CompraItemDto> Items { get; set; } = new List<CompraItemDto>();
    }
}