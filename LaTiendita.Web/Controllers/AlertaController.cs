using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class AlertaController : Controller
    {
        private readonly IProductoService _productoService;

        public AlertaController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        // GET: /Alerta
        public async Task<IActionResult> Index()
        {
            // LINQ: Traer solo productos con stock <= stock mínimo ordenados de más crítico a menos crítico
            var alertas = (await _productoService.GetStockBajoAsync())
                .OrderBy(p => p.Stock);

            return View(alertas);
        }
    }
}