using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthApiController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
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

            var empleado = await _authService.LoginAsync(dto.Correo, dto.Password);
            if (empleado == null)
            {
                return Unauthorized(new
                {
                    exito = false,
                    mensaje = "Correo o contraseña incorrectos, o usuario inactivo."
                });
            }

            var tokenString = _jwtService.GenerarToken(empleado);

            return Ok(new
            {
                exito = true,
                mensaje = "Autenticación exitosa.",
                token = tokenString,
                tipo = "Bearer",
                expiraEnHoras = 8,
                usuario = new
                {
                    id = empleado.Id,
                    nombre = empleado.Nombre,
                    correo = empleado.Correo,
                    rol = empleado.Rol.ToString()
                }
            });
        }

        [HttpGet("perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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