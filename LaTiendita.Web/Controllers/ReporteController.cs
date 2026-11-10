using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.Interfaces;

namespace LaTiendita.Web.Controllers
{
    // Solo los administradores pueden ver reportes y finanzas
    [Authorize(Roles = "Administrador")]
    public class ReporteController : Controller
    {
        private readonly IReporteService _reporteService;

        public ReporteController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        // GET: /Reporte o /Reporte?rango=semana
        public async Task<IActionResult> Index(string rango = "hoy")
        {
            var rangoFiltro = (rango ?? "hoy").ToLower();
            ViewBag.Rango = rangoFiltro;

            // 1. Obtener KPIs calculados con LINQ en el servicio
            var kpis = await _reporteService.ObtenerKpisAsync(rangoFiltro);

            // 2. Obtener historial de ventas del período seleccionado desde el servicio
            var ventasRecientes = await _reporteService.ObtenerVentasPorRangoAsync(rangoFiltro, 25);

            ViewBag.VentasRecientes = ventasRecientes;

            return View(kpis);
        }
    }
}