using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Enums;

namespace LaTiendita.Web.Controllers
{
    // Solo los administradores pueden acceder a este controlador
    [Authorize(Roles = "Administrador")]
    public class EmpleadoController : Controller
    {
        private readonly IEmpleadoService _empleadoService;

        public EmpleadoController(IEmpleadoService empleadoService)
        {
            _empleadoService = empleadoService;
        }

        // GET: /Empleado o /Empleado?id=1 (para editar)
        public async Task<IActionResult> Index(int? id)
        {
            var empleados = await _empleadoService.GetAllAsync();
            ViewBag.Empleados = empleados;

            var model = new EmpleadoDto();
            if (id.HasValue && id.Value > 0)
            {
                var emp = await _empleadoService.GetByIdAsync(id.Value);
                if (emp != null)
                {
                    model = new EmpleadoDto
                    {
                        Id = emp.Id,
                        Nombre = emp.Nombre,
                        Telefono = emp.Telefono,
                        Correo = emp.Correo,
                        Rol = emp.Rol
                    };
                }
            }

            return View(model);
        }

        // POST: /Empleado/Guardar (Crear o Editar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(EmpleadoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Correo))
            {
                TempData["Error"] = "El nombre y el correo son obligatorios.";
                return RedirectToAction(nameof(Index));
            }

            bool exito;
            if (dto.Id > 0)
            {
                exito = await _empleadoService.ActualizarAsync(dto);
                TempData[exito ? "Exito" : "Error"] = exito ? "Empleado actualizado con éxito." : "Error al actualizar empleado.";
            }
            else
            {
                exito = await _empleadoService.CrearAsync(dto);
                TempData[exito ? "Exito" : "Error"] = exito ? "Empleado registrado con éxito." : "Error al registrar empleado (¿correo ya registrado?).";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Empleado/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _empleadoService.EliminarAsync(id);
            TempData["Exito"] = "Empleado desactivado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}