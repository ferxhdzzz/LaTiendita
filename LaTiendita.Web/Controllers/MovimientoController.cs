using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaTiendita.Domain.Enums;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class MovimientoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovimientoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Movimiento (con filtros LINQ por tipo y búsqueda)
        public async Task<IActionResult> Index(TipoMovimiento? tipo, string? buscar)
        {
            var query = _context.Movimientos
                .Include(m => m.Producto)
                .Include(m => m.Empleado)
                .AsQueryable();

            // 1. LINQ: Filtro por tipo de movimiento (Entrada o Salida)
            if (tipo.HasValue)
            {
                query = query.Where(m => m.Tipo == tipo.Value);
            }

            // 2. LINQ: Filtro por texto de búsqueda
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                query = query.Where(m => m.Producto != null && m.Producto.Nombre.Contains(buscar));
            }

            // 3. LINQ: Ordenamiento cronológico descendente (los más recientes primero)
            var movimientos = await query
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            ViewBag.TipoSeleccionado = tipo;
            ViewBag.Buscar = buscar;

            return View(movimientos);
        }
    }
}