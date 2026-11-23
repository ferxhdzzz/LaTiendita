using System.ComponentModel.DataAnnotations;

namespace LaTiendita.Application.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El costo es obligatorio.")]
        [Range(0.01, 100000.00, ErrorMessage = "El costo debe ser mayor a 0.")]
        public decimal Costo { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 100000.00, ErrorMessage = "El precio de venta debe ser mayor a 0.")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, 100000, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Range(1, 10000, ErrorMessage = "El stock mínimo debe ser al menos 1.")]
        public int StockMinimo { get; set; } = 5;

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }

        public string? CategoriaNombre { get; set; }
        public int? ProveedorId { get; set; }
        public string? ProveedorNombre { get; set; }
    }
}