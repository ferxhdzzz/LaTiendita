using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            return await _unitOfWork.Productos.GetByIdAsync(id);
        }

        public async Task<bool> CrearAsync(ProductoDto dto)
        {
            // Validaciones de datos
            if (dto.Precio < 0 || dto.Costo < 0) return false;

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Costo = dto.Costo,
                Precio = dto.Precio,
                Stock = dto.Stock,
                StockMinimo = dto.StockMinimo,
                CategoriaId = dto.CategoriaId,
                ProveedorId = dto.ProveedorId,
                Activo = true
            };

            await _unitOfWork.Productos.AddAsync(producto);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarAsync(ProductoDto dto)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(dto.Id);
            if (producto == null) return false;

            producto.Nombre = dto.Nombre;
            producto.Costo = dto.Costo;
            producto.Precio = dto.Precio;
            producto.Stock = dto.Stock; // <-- ¡AQUÍ ESTÁ LA LÍNEA QUE FALTABA!
            producto.StockMinimo = dto.StockMinimo;
            producto.CategoriaId = dto.CategoriaId;
            producto.ProveedorId = dto.ProveedorId;

            _unitOfWork.Productos.Update(producto);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);
            if (producto == null) return false;

            producto.Activo = false; // Borrado lógico
            _unitOfWork.Productos.Update(producto);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }
    }
}