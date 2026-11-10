namespace LaTiendita.Application.DTOs
{
    public class CompraItemDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }

    public class CompraRegistroDto
    {
        public int? ProveedorId { get; set; }
        public int? EmpleadoId { get; set; }
        public List<CompraItemDto> Items { get; set; } = new List<CompraItemDto>();
    }
}