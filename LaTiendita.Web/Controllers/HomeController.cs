using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(
            IVentaService ventaService,
            IProductoService productoService,
            IUnitOfWork unitOfWork)
        {
            _ventaService = ventaService;
            _productoService = productoService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.UtcNow.Date;

            // 1. Ventas de hoy
            var ventasHoy = (await _ventaService.GetAllAsync())
                .Where(v => v.Fecha >= hoy)
                .ToList();

            ViewBag.TotalVentasHoy = ventasHoy.Sum(v => v.Total);
            ViewBag.CantidadVentasHoy = ventasHoy.Count;

            // 2. Productos por reponer (Stock bajo o agotado)
            var stockBajoList = (await _productoService.GetStockBajoAsync())
                .OrderBy(p => p.Stock)
                .ToList();

            ViewBag.PorReponer = stockBajoList.Take(5).ToList();
            ViewBag.TotalPorReponer = stockBajoList.Count;

            // 3. Total de productos en el catálogo
            var catalogoActivo = await _productoService.GetAllAsync();
            ViewBag.TotalProductos = catalogoActivo.Count();

            // 4. Últimos movimientos (Kardex)
            var ultimosMovimientos = (await _unitOfWork.Movimientos.GetMovimientosConDetallesAsync())
                .Take(5)
                .ToList();

            ViewBag.UltimosMovimientos = ultimosMovimientos;

            return View();
        }
    }
}