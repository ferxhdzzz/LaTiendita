namespace LaTiendita.Application.DTOs
{
    public class TopProductoDto
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalRecaudado { get; set; }
    }

    public class ReporteKpisDto
    {
        public decimal TotalVentas { get; set; }
        public decimal UtilidadTotal { get; set; }
        public int TotalTransacciones { get; set; }
        public int ProductosStockBajo { get; set; }
        public List<TopProductoDto> TopProductos { get; set; } = new List<TopProductoDto>();
    }
}