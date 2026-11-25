using Microsoft.Extensions.Logging;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(IUnitOfWork unitOfWork, ILogger<ProductoService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _unitOfWork.Productos.GetProductosConCategoriaAsync();
        }

        public async Task<IEnumerable<Producto>> GetStockBajoAsync()
        {
            return await _unitOfWork.Productos.GetProductosStockBajoAsync();
        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Productos.GetByIdConCategoriaAsync(id);
        }

        public async Task<Producto?> CrearAsync(ProductoDto dto)
        {
            if (dto.Precio <= 0 || dto.Costo <= 0 || dto.Stock < 0 || dto.StockMinimo < 1)
                return null;

            var producto = new Producto
            {
                Nombre = dto.Nombre.Trim(),
                Costo = dto.Costo,
                Precio = dto.Precio,
                Stock = dto.Stock,
                StockMinimo = dto.StockMinimo,
                CategoriaId = dto.CategoriaId,
                ProveedorId = dto.ProveedorId,
                Activo = true
            };

            await _unitOfWork.Productos.AddAsync(producto);
            var guardado = await _unitOfWork.SaveChangesAsync() > 0;
            if (!guardado) return null;

            _logger.LogInformation("Auditoría Producto: creado producto {ProductoId} ('{Nombre}') con stock {Stock} y precio {Precio}.",
                producto.Id, producto.Nombre, producto.Stock, producto.Precio);

            return await _unitOfWork.Productos.GetByIdConCategoriaAsync(producto.Id);
        }

        public async Task<Producto?> ActualizarAsync(ProductoDto dto)
        {
            if (dto.Precio <= 0 || dto.Costo <= 0 || dto.Stock < 0 || dto.StockMinimo < 1)
                return null;

            var producto = await _unitOfWork.Productos.GetByIdAsync(dto.Id);
            if (producto == null || !producto.Activo) return null;

            producto.Nombre = dto.Nombre.Trim();
            producto.Costo = dto.Costo;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock;
            producto.StockMinimo = dto.StockMinimo;
            producto.CategoriaId = dto.CategoriaId;
            if (dto.ProveedorId.HasValue)
            {
                producto.ProveedorId = dto.ProveedorId;
            }

            _unitOfWork.Productos.Update(producto);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Auditoría Producto: actualizado producto {ProductoId} ('{Nombre}') con stock {Stock} y precio {Precio}.",
                producto.Id, producto.Nombre, producto.Stock, producto.Precio);

            return await _unitOfWork.Productos.GetByIdConCategoriaAsync(producto.Id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null || !producto.Activo) return false;

            producto.Activo = false; // Borrado lógico
            _unitOfWork.Productos.Update(producto);
            var eliminado = await _unitOfWork.SaveChangesAsync() > 0;

            if (eliminado)
            {
                _logger.LogInformation("Auditoría Producto: desactivado producto {ProductoId} ('{Nombre}').",
                    producto.Id, producto.Nombre);
            }

            return eliminado;
        }
    }
}