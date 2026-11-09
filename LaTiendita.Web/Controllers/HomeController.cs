using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.UtcNow.Date;

            // 1. Ventas de hoy
            var ventasHoy = await _context.Ventas
                .Where(v => v.Fecha >= hoy)
                .ToListAsync();

            ViewBag.TotalVentasHoy = ventasHoy.Sum(v => v.Total);
            ViewBag.CantidadVentasHoy = ventasHoy.Count;

            // 2. Productos por reponer (Stock bajo o agotado)
            var porReponer = await _context.Productos
                .Where(p => p.Activo && p.Stock <= p.StockMinimo)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            ViewBag.PorReponer = porReponer;
            ViewBag.TotalPorReponer = await _context.Productos.CountAsync(p => p.Activo && p.Stock <= p.StockMinimo);

            // 3. Total de productos en el catálogo
            ViewBag.TotalProductos = await _context.Productos.CountAsync(p => p.Activo);

            // 4. Últimos movimientos (Kardex)
            var ultimosMovimientos = await _context.Movimientos
                .Include(m => m.Producto)
                .Include(m => m.Empleado)
                .OrderByDescending(m => m.Fecha)
                .Take(5)
                .ToListAsync();

            ViewBag.UltimosMovimientos = ultimosMovimientos;

            return View();
        }
    }
}