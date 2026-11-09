using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaTiendita.Application.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Web.Controllers
{
    // Solo los administradores pueden ver reportes y finanzas
    [Authorize(Roles = "Administrador")]
    public class ReporteController : Controller
    {
        private readonly IReporteService _reporteService;
        private readonly ApplicationDbContext _context;

        public ReporteController(IReporteService reporteService, ApplicationDbContext context)
        {
            _reporteService = reporteService;
            _context = context;
        }

        // GET: /Reporte o /Reporte?rango=semana
        public async Task<IActionResult> Index(string rango = "hoy")
        {
            var rangoFiltro = (rango ?? "hoy").ToLower();
            ViewBag.Rango = rangoFiltro;

            // 1. Obtener KPIs calculados con LINQ en el servicio
            var kpis = await _reporteService.ObtenerKpisAsync(rangoFiltro);

            // 2. Consulta LINQ para el historial de ventas del período seleccionado
            var query = _context.Ventas
                .Include(v => v.Empleado)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .AsQueryable();

            var ahora = DateTime.UtcNow;
            switch (rangoFiltro)
            {
                case "hoy":
                    var hoy = ahora.Date;
                    query = query.Where(v => v.Fecha >= hoy);
                    break;
                case "semana":
                    var semana = ahora.Date.AddDays(-7);
                    query = query.Where(v => v.Fecha >= semana);
                    break;
                case "mes":
                    var mes = new DateTime(ahora.Year, ahora.Month, 1);
                    query = query.Where(v => v.Fecha >= mes);
                    break;
                default:
                    // "todo" - sin filtro de fecha
                    break;
            }

            var ventasRecientes = await query
                .OrderByDescending(v => v.Fecha)
                .Take(25)
                .ToListAsync();

            ViewBag.VentasRecientes = ventasRecientes;

            return View(kpis);
        }
    }
}