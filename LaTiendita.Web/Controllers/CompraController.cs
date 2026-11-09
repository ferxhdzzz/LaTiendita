using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class CompraController : Controller
    {
        private readonly ICompraService _compraService;
        private readonly IProductoService _productoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public CompraController(
            ICompraService compraService,
            IProductoService productoService,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
        {
            _compraService = compraService;
            _productoService = productoService;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        // GET: /Compra (Con LINQ Include para traer detalles, productos y proveedor)
        public async Task<IActionResult> Index()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Empleado)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();

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