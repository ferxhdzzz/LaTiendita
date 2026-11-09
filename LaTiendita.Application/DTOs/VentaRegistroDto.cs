namespace LaTiendita.Application.DTOs
{
    public class VentaItemDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class VentaRegistroDto
    {
        public int? EmpleadoId { get; set; }
        public List<VentaItemDto> Items { get; set; } = new List<VentaItemDto>();
    }
}