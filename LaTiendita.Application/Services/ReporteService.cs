using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class ReporteService : IReporteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReporteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static DateTime ObtenerFechaInicio(string rango)
        {
            var ahora = DateTime.UtcNow;
            return (rango ?? "hoy").ToLower() switch
            {
                "hoy" => ahora.Date,
                "semana" => ahora.Date.AddDays(-7),
                "mes" => new DateTime(ahora.Year, ahora.Month, 1),
                _ => DateTime.MinValue // "todo"
            };
        }

        public async Task<ReporteKpisDto> ObtenerKpisAsync(string rango)
        {
            // 1. LINQ - Determinar fecha inicial del filtro
            DateTime fechaInicio = ObtenerFechaInicio(rango);
            DateTime fechaFin = DateTime.UtcNow;

            // 2. LINQ - Filtro por rango de fechas
            var ventas = (await _unitOfWork.Ventas.GetVentasPorRangoFechaAsync(fechaInicio, fechaFin)).ToList();

            // 3. LINQ - Sumatorias y agregaciones
            decimal totalVentas = ventas.Sum(v => v.Total);

            // LINQ - Cálculo de costo para obtener la utilidad real (Ingresos - Costos)
            decimal costoTotal = ventas
                .SelectMany(v => v.Detalles)
                .Sum(d => d.Cantidad * d.CostoUnitario);

            decimal utilidadTotal = totalVentas - costoTotal;
            int totalTransacciones = ventas.Count;

            // Alertas de stock bajo
            var productosStockBajo = await _unitOfWork.Productos.GetProductosStockBajoAsync();

            // 4. LINQ - Agrupación (GroupBy) y ordenamiento para el Top de Productos
            var topProductos = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => new { d.ProductoId, Nombre = d.Producto != null ? d.Producto.Nombre : $"Producto #{d.ProductoId}" })
                .Select(g => new TopProductoDto
                {
                    NombreProducto = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalRecaudado = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(tp => tp.CantidadVendida)
                .Take(5)
                .ToList();

            return new ReporteKpisDto
            {
                TotalVentas = totalVentas,
                UtilidadTotal = utilidadTotal,
                TotalTransacciones = totalTransacciones,
                ProductosStockBajo = productosStockBajo.Count(),
                TopProductos = topProductos
            };
        }

        public async Task<IEnumerable<TopProductoDto>> ObtenerTopProductosVendidosAsync(int top = 5)
        {
            var ventas = await _unitOfWork.Ventas.GetVentasConDetallesAsync();

            return ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => new { d.ProductoId, Nombre = d.Producto != null ? d.Producto.Nombre : $"Producto #{d.ProductoId}" })
                .Select(g => new TopProductoDto
                {
                    NombreProducto = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalRecaudado = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(tp => tp.CantidadVendida)
                .Take(top)
                .ToList();
        }

        public async Task<IEnumerable<Venta>> ObtenerVentasPorRangoAsync(string rango, int limite = 25)
        {
            DateTime fechaInicio = ObtenerFechaInicio(rango);
            DateTime fechaFin = DateTime.UtcNow;

            var ventas = await _unitOfWork.Ventas.GetVentasPorRangoFechaAsync(fechaInicio, fechaFin);
            return ventas.Take(limite).ToList();
        }
    }
}