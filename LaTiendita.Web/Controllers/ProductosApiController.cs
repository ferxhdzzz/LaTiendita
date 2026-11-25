using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Web.Controllers
{
    /// <summary>
    /// Controlador RESTful para el mantenimiento CRUD del catálogo de productos.
    /// </summary>
    [ApiController]
    [Route("api/productos")]
    [Produces("application/json")]
    public class ProductosApiController : ControllerBase
    {
        private const string EsquemasAuth = JwtBearerDefaults.AuthenticationScheme + ",Identity.Application";

        private readonly IProductoService _productoService;
        private readonly ILogger<ProductosApiController> _logger;

        public ProductosApiController(IProductoService productoService, ILogger<ProductosApiController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        private static ProductoDto MapearDto(Producto p) => new ProductoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Costo = p.Costo,
            Precio = p.Precio,
            Stock = p.Stock,
            StockMinimo = p.StockMinimo,
            CategoriaId = p.CategoriaId,
            CategoriaNombre = p.Categoria?.Nombre,
            ProveedorId = p.ProveedorId,
            ProveedorNombre = p.Proveedor?.Nombre
        };

        /// <summary>
        /// Obtiene el listado completo de productos activos en el inventario.
        /// </summary>
        /// <response code="200">Retorna la lista de productos activos.</response>
        /// <response code="401">El usuario no está autenticado.</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = EsquemasAuth, Policy = "PersonalTienda")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var productos = (await _productoService.GetAllAsync())
                .Select(MapearDto)
                .ToList();

            return Ok(new
            {
                exito = true,
                total = productos.Count,
                datos = productos
            });
        }

        /// <summary>
        /// Consulta un producto activo específico por su identificador.
        /// </summary>
        /// <param name="id">Identificador único del producto.</param>
        /// <response code="200">Producto encontrado.</response>
        /// <response code="401">El usuario no está autenticado.</response>
        /// <response code="404">No existe un producto activo con el ID indicado.</response>
        [HttpGet("{id:int}")]
        [Authorize(AuthenticationSchemes = EsquemasAuth, Policy = "PersonalTienda")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null || !producto.Activo)
            {
                return NotFound(new { exito = false, mensaje = $"Producto con ID {id} no encontrado." });
            }

            return Ok(new { exito = true, dato = MapearDto(producto) });
        }

        /// <summary>
        /// Registra un nuevo producto en el catálogo (requiere rol Administrador).
        /// </summary>
        /// <param name="dto">Datos del producto a crear.</param>
        /// <response code="201">Producto creado exitosamente.</response>
        /// <response code="400">Errores de validación en los datos enviados.</response>
        /// <response code="401">El usuario no está autenticado.</response>
        /// <response code="403">El usuario no cuenta con rol Administrador.</response>
        [HttpPost]
        [Authorize(AuthenticationSchemes = EsquemasAuth, Policy = "SoloAdmin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
            if (creado == null)
            {
                return BadRequest(new { exito = false, mensaje = "No se pudo registrar el producto. Verifica los valores enviados." });
            }

            var resultado = MapearDto(creado);
            _logger.LogInformation("API POST /api/productos: usuario {Usuario} creó producto {ProductoId}.", User.Identity?.Name, creado.Id);

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, new
            {
                exito = true,
                mensaje = "Producto creado exitosamente.",
                dato = resultado
            });
        }

        /// <summary>
        /// Actualiza los datos de un producto existente (requiere rol Administrador).
        /// </summary>
        /// <param name="id">Identificador del producto en la ruta.</param>
        /// <param name="dto">Nuevos datos del producto.</param>
        /// <response code="200">Producto actualizado correctamente.</response>
        /// <response code="400">El ID no coincide o hay errores de validación.</response>
        /// <response code="401">El usuario no está autenticado.</response>
        /// <response code="403">El usuario no cuenta con rol Administrador.</response>
        /// <response code="404">Producto no encontrado.</response>
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = EsquemasAuth, Policy = "SoloAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            if (actualizado == null)
            {
                return NotFound(new { exito = false, mensaje = $"Producto con ID {id} no encontrado o no se pudo actualizar." });
            }

            _logger.LogInformation("API PUT /api/productos/{ProductoId}: actualizado por {Usuario}.", id, User.Identity?.Name);

            return Ok(new
            {
                exito = true,
                mensaje = "Producto actualizado exitosamente.",
                dato = MapearDto(actualizado)
            });
        }

        /// <summary>
        /// Desactiva (borrado lógico) un producto del catálogo (requiere rol Administrador).
        /// </summary>
        /// <param name="id">Identificador del producto a eliminar.</param>
        /// <response code="204">Producto eliminado correctamente (sin contenido).</response>
        /// <response code="401">El usuario no está autenticado.</response>
        /// <response code="403">El usuario no cuenta con rol Administrador.</response>
        /// <response code="404">Producto no encontrado.</response>
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = EsquemasAuth, Policy = "SoloAdmin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _productoService.EliminarAsync(id);
            if (!eliminado)
            {
                return NotFound(new { exito = false, mensaje = $"Producto con ID {id} no encontrado." });
            }

            _logger.LogInformation("API DELETE /api/productos/{ProductoId}: eliminado por {Usuario}.", id, User.Identity?.Name);
            return NoContent();
        }
    }
}