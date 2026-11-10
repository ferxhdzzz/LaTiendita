using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class CompraController : Controller
    {
        private readonly ICompraService _compraService;
        private readonly IProductoService _productoService;
        private readonly IUnitOfWork _unitOfWork;

        public CompraController(
            ICompraService compraService,
            IProductoService productoService,
            IUnitOfWork unitOfWork)
        {
            _compraService = compraService;
            _productoService = productoService;
            _unitOfWork = unitOfWork;
        }

        // GET: /Compra (Con detalles, productos y proveedor mediante servicio)
        public async Task<IActionResult> Index()
        {
            var compras = await _compraService.GetAllAsync();

            var productos = (await _productoService.GetAllAsync()).Where(p => p.Activo);
            var proveedores = await _unitOfWork.Proveedores.GetAllAsync();

            ViewBag.Productos = productos;
            ViewBag.Proveedores = proveedores;

            return View(compras);
        }

        // POST: /Compra/RegistrarCompraJson
        [HttpPost]
        public async Task<IActionResult> RegistrarCompraJson([FromBody] CompraRegistroDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                return Json(new { exito = false, mensaje = "El detalle de compra está vacío. Agrega al menos un producto." });
            }

            int? empleadoId = null;
            var empClaim = User.FindFirst("EmpleadoId")?.Value;
            if (int.TryParse(empClaim, out int idVal))
            {
                empleadoId = idVal;
            }
            dto.EmpleadoId = empleadoId;

            var resultado = await _compraService.RegistrarCompraAsync(dto);

            return Json(new { exito = resultado.Exito, mensaje = resultado.Mensaje });
        }
    }
}