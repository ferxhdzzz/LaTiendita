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

        public async Task<IActionResult> Index(string rango = "hoy")
        {
            var rangoFiltro = (rango ?? "hoy").ToLower();
            ViewBag.Rango = rangoFiltro;

            var kpis = await _reporteService.ObtenerKpisAsync(rangoFiltro);
            var ventasRecientes = await _reporteService.ObtenerVentasPorRangoAsync(rangoFiltro, 25);

            ViewBag.VentasRecientes = ventasRecientes;

            return View(kpis);
        }
    }
}