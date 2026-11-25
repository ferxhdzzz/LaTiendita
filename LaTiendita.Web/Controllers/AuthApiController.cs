using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Infrastructure.Identity;

namespace LaTiendita.Web.Controllers
{
    /// <summary>
    /// Controlador REST de autenticación mediante ASP.NET Core Identity y JWT Bearer.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            ILogger<AuthApiController> logger)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica un usuario registrado en Identity y genera un token JWT Bearer.
        /// </summary>
        /// <param name="dto">Credenciales de inicio de sesión (correo y contraseña).</param>
        /// <response code="200">Credenciales válidas; devuelve el token JWT y datos del usuario.</response>
        /// <response code="400">Los datos enviados no cumplen con las validaciones.</response>
        /// <response code="401">Correo o contraseña incorrectos, o usuario inactivo.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginApiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Datos de entrada inválidos.",
                    errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var user = await _userManager.FindByEmailAsync(dto.Correo.Trim());
            if (user == null || !user.Activo || await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login API fallido para {Correo}: usuario no encontrado o inactivo.", dto.Correo);
                return Unauthorized(new
                {
                    exito = false,
                    mensaje = "Correo o contraseña incorrectos, o usuario inactivo."
                });
            }

            var passwordValida = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValida)
            {
                _logger.LogWarning("Login API fallido para {Correo}: contraseña inválida.", dto.Correo);
                return Unauthorized(new
                {
                    exito = false,
                    mensaje = "Correo o contraseña incorrectos, o usuario inactivo."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokenString = _jwtService.GenerarToken(
                user.Id,
                user.NombreCompleto,
                user.Email ?? dto.Correo,
                roles,
                user.EmpleadoId);

            _logger.LogInformation("Token JWT emitido correctamente para {Correo} (Roles: {Roles}).",
                user.Email, string.Join(",", roles));

            return Ok(new
            {
                exito = true,
                mensaje = "Autenticación exitosa.",
                token = tokenString,
                tipo = "Bearer",
                expiraEnHoras = _jwtService.DurationInHours,
                usuario = new
                {
                    id = user.Id,
                    empleadoId = user.EmpleadoId,
                    nombre = user.NombreCompleto,
                    correo = user.Email,
                    rol = roles.FirstOrDefault() ?? "Empleado"
                }
            });
        }

        /// <summary>
        /// Devuelve los datos del usuario autenticado extraídos del token JWT.
        /// </summary>
        /// <response code="200">Token JWT válido; retorna la información del perfil.</response>
        /// <response code="401">No se proporcionó un token JWT válido.</response>
        [HttpGet("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ObtenerPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var nombre = User.FindFirstValue(ClaimTypes.Name);
            var correo = User.FindFirstValue(ClaimTypes.Email);
            var rol = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                exito = true,
                mensaje = "Acceso concedido mediante JWT válido.",
                usuario = new
                {
                    id = userId,
                    nombre = nombre,
                    correo = correo,
                    rol = rol
                }
            });
        }
    }
}