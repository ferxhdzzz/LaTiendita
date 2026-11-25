using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(
            IProductoService productoService,
            IUnitOfWork unitOfWork)
        {
            _productoService = productoService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var hoyLocal = DateTime.Today;
            var inicioHoyUtc = hoyLocal.ToUniversalTime();
            var finHoyUtc = hoyLocal.AddDays(1).ToUniversalTime();

            var ventasHoy = (await _unitOfWork.Ventas.GetVentasPorRangoFechaAsync(inicioHoyUtc, finHoyUtc.AddTicks(-1)))
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
            var fechaLimiteVencimiento = hoyLocal.AddDays(30);
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