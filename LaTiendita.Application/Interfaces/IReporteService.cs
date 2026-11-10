using LaTiendita.Application.DTOs;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Application.Interfaces
{
    public interface IReporteService
    {
        Task<ReporteKpisDto> ObtenerKpisAsync(string rango); // "hoy", "semana", "mes", "todo"
        Task<IEnumerable<TopProductoDto>> ObtenerTopProductosVendidosAsync(int top = 5);
        Task<IEnumerable<Venta>> ObtenerVentasPorRangoAsync(string rango, int limite = 25);
    }
}