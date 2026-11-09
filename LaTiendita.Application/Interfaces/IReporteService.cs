using LaTiendita.Application.DTOs;

namespace LaTiendita.Application.Interfaces
{
    public interface IReporteService
    {
        Task<ReporteKpisDto> ObtenerKpisAsync(string rango); // "hoy", "semana", "mes", "todo"
        Task<IEnumerable<TopProductoDto>> ObtenerTopProductosVendidosAsync(int top = 5);
    }
}