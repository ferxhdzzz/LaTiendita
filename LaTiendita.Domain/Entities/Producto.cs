
namespace LaTiendita.Domain.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Costo { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; } = true;

        // Clave foránea y relación con Categoría
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // Proveedor habitual (opcional)
        public int? ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }
    }
}