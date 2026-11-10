using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Domain.Enums;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class MovimientoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MovimientoController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Movimiento (con filtros LINQ por tipo y búsqueda)
        public async Task<IActionResult> Index(TipoMovimiento? tipo, string? buscar)
        {
            var movimientos = await _unitOfWork.Movimientos.GetMovimientosConDetallesAsync(tipo, buscar);

            ViewBag.TipoSeleccionado = tipo;
            ViewBag.Buscar = buscar;

            return View(movimientos);
        }
    }
}