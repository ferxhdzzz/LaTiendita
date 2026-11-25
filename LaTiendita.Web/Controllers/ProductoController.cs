using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductoController(IProductoService productoService, IUnitOfWork unitOfWork)
        {
            _productoService = productoService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string? buscar, int? categoriaId, string? existencia)
        {
            var productos = await _productoService.GetAllAsync();
            var categorias = await _unitOfWork.Categorias.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();
                productos = productos.Where(p =>
                    p.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    $"P-{p.Id:D4}".Contains(buscar, StringComparison.OrdinalIgnoreCase));
            }

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                productos = productos.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrEmpty(existencia))
            {
                productos = existencia.ToLower() switch
                {
                    "suficiente" => productos.Where(p => p.Stock > p.StockMinimo),
                    "bajo" => productos.Where(p => p.Stock > 0 && p.Stock <= p.StockMinimo),
                    "agotado" => productos.Where(p => p.Stock == 0),
                    _ => productos
                };
            }

            ViewBag.Categorias = categorias;
            ViewBag.Buscar = buscar;
            ViewBag.CategoriaId = categoriaId;
            ViewBag.Existencia = existencia;

            return View(productos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(ProductoDto dto)
        {
            if (!ModelState.IsValid)
            {
                var primerError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

                TempData["Error"] = primerError ?? "Verifica los datos ingresados del producto.";
                return RedirectToAction(nameof(Index));
            }

            bool exito;
            if (dto.Id > 0)
            {
                exito = await _productoService.ActualizarAsync(dto);
                TempData[exito ? "Exito" : "Error"] = exito ? "Producto actualizado correctamente." : "Error al actualizar.";
            }
            else
            {
                exito = await _productoService.CrearAsync(dto);
                TempData[exito ? "Exito" : "Error"] = exito ? "Producto agregado al catálogo." : "Error al registrar.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Producto/Eliminar
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _productoService.EliminarAsync(id);
            TempData["Exito"] = "Producto eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}