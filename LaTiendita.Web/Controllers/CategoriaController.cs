using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _unitOfWork.Categorias.GetAllAsync();
            return View(categorias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Categoria model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                TempData["Error"] = "El nombre es obligatorio.";
                return RedirectToAction(nameof(Index));
            }

            if (model.Id > 0)
            {
                _unitOfWork.Categorias.Update(model);
            }
            else
            {
                await _unitOfWork.Categorias.AddAsync(model);
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Exito"] = "Categoría guardada con éxito.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var cat = await _unitOfWork.Categorias.GetByIdAsync(id);
                if (cat != null)
                {
                    // Validar si hay productos usando esta categoría
                    var tieneProductos = (await _unitOfWork.Productos.FindAsync(p => p.CategoriaId == id && p.Activo)).Any();

                    if (tieneProductos)
                    {
                        TempData["Error"] = "No se puede eliminar esta categoría porque contiene productos en el catálogo.";
                        return RedirectToAction(nameof(Index));
                    }

                    _unitOfWork.Categorias.Delete(cat);
                    await _unitOfWork.SaveChangesAsync();
                    TempData["Exito"] = "Categoría eliminada con éxito.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error al intentar eliminar la categoría.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}