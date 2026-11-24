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

            var ventasHoy = (await _ventaService.GetAllAsync())
                .Where(v => v.Fecha >= hoy)
                .ToList();

            ViewBag.TotalVentasHoy = ventasHoy.Sum(v => v.Total);
            ViewBag.CantidadVentasHoy = ventasHoy.Count;

            var stockBajoList = (await _productoService.GetStockBajoAsync())
                .OrderBy(p => p.Stock)
                .ToList();

            ViewBag.PorReponer = stockBajoList.Take(5).ToList();
            ViewBag.TotalPorReponer = stockBajoList.Count;

            // Lotes vencidos o próximos a vencer (próximos 30 días)
            var compras = await _unitOfWork.Compras.GetComprasConDetallesAsync();
            var fechaLimiteVencimiento = hoy.AddDays(30);
            var lotesPorVencer = compras
                .SelectMany(c => c.Detalles)
                .Where(d => d.FechaVencimiento.HasValue
                            && (d.Producto == null || d.Producto.Activo)
                            && d.FechaVencimiento.Value.Date <= fechaLimiteVencimiento)
                .OrderBy(d => d.FechaVencimiento!.Value)
                .ToList();

            ViewBag.TotalVencimientos = lotesPorVencer.Count;
            ViewBag.LotesPorVencer = lotesPorVencer.Take(5).ToList();

            var catalogoActivo = await _productoService.GetAllAsync();
            ViewBag.TotalProductos = catalogoActivo.Count();

            var ultimosMovimientos = (await _unitOfWork.Movimientos.GetMovimientosConDetallesAsync())
                .Take(5)
                .ToList();

            ViewBag.UltimosMovimientos = ultimosMovimientos;

            return View();
        }
    }
}