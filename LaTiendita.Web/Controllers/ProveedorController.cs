using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [Authorize]
    public class ProveedorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProveedorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var proveedores = await _unitOfWork.Proveedores.GetAllAsync();
            return View(proveedores);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(Proveedor model)
        {
            // Validación estricta: Ningún campo puede estar vacío
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Telefono) ||
                string.IsNullOrWhiteSpace(model.Correo) ||
                string.IsNullOrWhiteSpace(model.LineasSuministro))
            {
                TempData["Error"] = "Todos los campos del proveedor son obligatorios. No se permiten datos vacíos.";
                return RedirectToAction(nameof(Index));
            }

            if (model.Id > 0)
            {
                _unitOfWork.Proveedores.Update(model);
            }
            else
            {
                await _unitOfWork.Proveedores.AddAsync(model);
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Exito"] = "Proveedor guardado con éxito.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var prov = await _unitOfWork.Proveedores.GetByIdAsync(id);
                if (prov != null)
                {
                    // 1. Validar integridad: ¿Tiene compras o productos asociados?
                    var tieneCompras = (await _unitOfWork.Compras.FindAsync(c => c.ProveedorId == id)).Any();
                    var tieneProductos = (await _unitOfWork.Productos.FindAsync(p => p.ProveedorId == id)).Any();

                    if (tieneCompras || tieneProductos)
                    {
                        TempData["Error"] = "No se puede eliminar este proveedor porque ya tiene compras o productos registrados en el historial.";
                        return RedirectToAction(nameof(Index));
                    }

                    _unitOfWork.Proveedores.Delete(prov);
                    await _unitOfWork.SaveChangesAsync();
                    TempData["Exito"] = "Proveedor eliminado con éxito.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "No se pudo eliminar el proveedor debido a registros asociados en la base de datos.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}