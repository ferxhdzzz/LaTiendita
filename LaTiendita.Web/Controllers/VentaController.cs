using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly IVentaService _ventaService;
        private readonly IProductoService _productoService;

        public VentaController(IVentaService ventaService, IProductoService productoService)
        {
            _ventaService = ventaService;
            _productoService = productoService;
        }

        // GET: /Venta
        public async Task<IActionResult> Index()
        {
            var ventas = await _ventaService.GetAllAsync();
            var productos = (await _productoService.GetAllAsync()).Where(p => p.Activo);

            ViewBag.Productos = productos;
            return View(ventas);
        }

        // POST: /Venta/RegistrarVentaJson (Procesa el Ticket completo con múltiples productos)
        [HttpPost]
        public async Task<IActionResult> RegistrarVentaJson([FromBody] VentaRegistroDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                return Json(new { exito = false, mensaje = "El ticket está vacío. Agrega al menos un producto." });
            }

            int? empleadoId = null;
            var empClaim = User.FindFirst("EmpleadoId")?.Value;
            if (int.TryParse(empClaim, out int idVal))
            {
                empleadoId = idVal;
            }
            dto.EmpleadoId = empleadoId;

            // Transacción atómica en Application
            var resultado = await _ventaService.RegistrarVentaAsync(dto);

            return Json(new { exito = resultado.Exito, mensaje = resultado.Mensaje });
        }
    }
}