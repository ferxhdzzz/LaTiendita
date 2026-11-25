using System.ComponentModel.DataAnnotations;

namespace LaTiendita.Application.DTOs
{
    public class VentaItemDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El identificador de producto no es válido.")]
        public int ProductoId { get; set; }

        [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }

    public class VentaRegistroDto
    {
        public int? EmpleadoId { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos un producto en la venta.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en la venta.")]
        public List<VentaItemDto> Items { get; set; } = new List<VentaItemDto>();
    }
}