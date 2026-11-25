using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Infrastructure.Identity;

namespace LaTiendita.Web.Controllers
{
    // Solo los administradores pueden acceder a este controlador
    [Authorize(Roles = "Administrador")]
    public class EmpleadoController : Controller
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EmpleadoController> _logger;

        public EmpleadoController(
            IEmpleadoService empleadoService,
            UserManager<ApplicationUser> userManager,
            ILogger<EmpleadoController> logger)
        {
            _empleadoService = empleadoService;
            _userManager = userManager;
            _logger = logger;
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

        // POST: /Empleado/Guardar (Crear o Editar sincronizado con ASP.NET Core Identity)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(EmpleadoDto dto)
        {
            if (!ModelState.IsValid)
            {
                var primerError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

                TempData["Error"] = primerError ?? "Verifica los datos del empleado.";
                return RedirectToAction(nameof(Index));
            }

            var correoLimpio = dto.Correo.Trim();
            var rolTexto = dto.Rol.ToString();

            if (dto.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                {
                    TempData["Error"] = "La contraseña es obligatoria y debe tener al menos 6 caracteres (incluyendo un número).";
                    return RedirectToAction(nameof(Index));
                }

                var existenteIdentity = await _userManager.FindByEmailAsync(correoLimpio);
                if (existenteIdentity != null)
                {
                    TempData["Error"] = "Ya existe una cuenta registrada con ese correo electrónico.";
                    return RedirectToAction(nameof(Index));
                }

                var empleadoCreado = await _empleadoService.CrearAsync(dto);
                if (empleadoCreado == null)
                {
                    TempData["Error"] = "No se pudo registrar el empleado (¿correo ya registrado?).";
                    return RedirectToAction(nameof(Index));
                }

                var nuevoUsuario = new ApplicationUser
                {
                    UserName = correoLimpio,
                    Email = correoLimpio,
                    EmailConfirmed = true,
                    PhoneNumber = dto.Telefono.Trim(),
                    NombreCompleto = dto.Nombre.Trim(),
                    Activo = true,
                    EmpleadoId = empleadoCreado.Id
                };

                var resultadoIdentity = await _userManager.CreateAsync(nuevoUsuario, dto.Password);
                if (!resultadoIdentity.Succeeded)
                {
                    await _empleadoService.EliminarAsync(empleadoCreado.Id);
                    var errores = string.Join(" ", resultadoIdentity.Errors.Select(e => e.Description));
                    TempData["Error"] = $"No se pudo crear la cuenta en Identity: {errores}";
                    return RedirectToAction(nameof(Index));
                }

                await _userManager.AddToRoleAsync(nuevoUsuario, rolTexto);
                _logger.LogInformation("Usuario Identity creado para {Correo} con rol {Rol}.", correoLimpio, rolTexto);

                TempData["Exito"] = "Empleado y cuenta de usuario registrados con éxito.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var empleadoAnterior = await _empleadoService.GetByIdAsync(dto.Id);
                if (empleadoAnterior == null)
                {
                    TempData["Error"] = "Empleado no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var correoAnterior = empleadoAnterior.Correo;
                var actualizado = await _empleadoService.ActualizarAsync(dto);
                if (!actualizado)
                {
                    TempData["Error"] = "Error al actualizar empleado (¿correo duplicado?).";
                    return RedirectToAction(nameof(Index));
                }

                // Sincronizar usuario y rol en ASP.NET Core Identity
                var user = await _userManager.FindByEmailAsync(correoAnterior);
                if (user != null)
                {
                    user.NombreCompleto = dto.Nombre.Trim();
                    user.PhoneNumber = dto.Telefono.Trim();
                    user.Email = correoLimpio;
                    user.UserName = correoLimpio;

                    var resUpdate = await _userManager.UpdateAsync(user);
                    if (!resUpdate.Succeeded)
                    {
                        var errores = string.Join(" ", resUpdate.Errors.Select(e => e.Description));
                        TempData["Error"] = $"Empleado actualizado, pero hubo un error en Identity: {errores}";
                        return RedirectToAction(nameof(Index));
                    }

                    var rolesActuales = await _userManager.GetRolesAsync(user);
                    if (!rolesActuales.Contains(rolTexto))
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesActuales);
                        await _userManager.AddToRoleAsync(user, rolTexto);
                    }

                    if (!string.IsNullOrWhiteSpace(dto.Password))
                    {
                        var tokenReset = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var resPass = await _userManager.ResetPasswordAsync(user, tokenReset, dto.Password);
                        if (!resPass.Succeeded)
                        {
                            var errores = string.Join(" ", resPass.Errors.Select(e => e.Description));
                            TempData["Error"] = $"Datos actualizados, pero no se pudo cambiar la contraseña: {errores}";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    _logger.LogInformation("Usuario Identity actualizado para {Correo} (Rol: {Rol}).", correoLimpio, rolTexto);
                }

                TempData["Exito"] = "Empleado actualizado con éxito.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Empleado/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var empClaim = User.FindFirst("EmpleadoId")?.Value;
            if (int.TryParse(empClaim, out int empleadoActualId) && empleadoActualId == id)
            {
                TempData["Error"] = "No puedes desactivar tu propia cuenta mientras tienes la sesión iniciada.";
                return RedirectToAction(nameof(Index));
            }

            var empleado = await _empleadoService.GetByIdAsync(id);
            await _empleadoService.EliminarAsync(id);

            if (empleado != null)
            {
                var user = await _userManager.FindByEmailAsync(empleado.Correo);
                if (user != null)
                {
                    user.Activo = false;
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("Usuario Identity bloqueado/desactivado para {Correo}.", empleado.Correo);
                }
            }

            TempData["Exito"] = "Empleado desactivado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}