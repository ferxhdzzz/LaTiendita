using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;

namespace LaTiendita.Web.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosApiController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosApiController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _productoService.GetAllAsync();
            return Ok(new
            {
                exito = true,
                total = productos.Count(),
                datos = productos
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null)
            {
                return NotFound(new { exito = false, mensaje = $"Producto con ID {id} no encontrado." });
            }

            return Ok(new { exito = true, dato = producto });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "SoloAdmin")]
        public async Task<IActionResult> Create([FromBody] ProductoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Errores de validación en los datos enviados.",
                    errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var creado = await _productoService.CrearAsync(dto);
            if (!creado)
            {
                return StatusCode(500, new { exito = false, mensaje = "Error al persistir el producto en la base de datos." });
            }

            return StatusCode(201, new
            {
                exito = true,
                mensaje = "Producto creado exitosamente.",
                dato = dto
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "SoloAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductoDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { exito = false, mensaje = "El ID de la ruta no coincide con el cuerpo de la petición." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    exito = false,
                    mensaje = "Errores de validación.",
                    errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var actualizado = await _productoService.ActualizarAsync(dto);
            if (!actualizado)
            {
                return NotFound(new { exito = false, mensaje = $"Producto con ID {id} no encontrado o no se pudo actualizar." });
            }

            return Ok(new
            {
                exito = true,
                mensaje = "Producto actualizado exitosamente.",
                dato = dto
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme, Policy = "SoloAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _productoService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound(new { exito = false, mensaje = $"Producto con ID {id} no encontrado." });
            }

            return Ok(new
            {
                exito = true,
                mensaje = $"Producto con ID {id} eliminado correctamente."
            });
        }
    }
}