using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Enums;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class CompraService : ICompraService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompraService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Compra>> GetAllAsync()
        {
            return await _unitOfWork.Compras.GetComprasConDetallesAsync();
        }

        public async Task<Compra?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Compras.GetCompraConDetallesAsync(id);
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarCompraAsync(CompraRegistroDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return (false, "Debe incluir al menos un producto en la compra.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var compra = new Compra
                {
                    Fecha = DateTime.UtcNow,
                    ProveedorId = dto.ProveedorId,
                    EmpleadoId = dto.EmpleadoId,
                    Total = 0
                };

                decimal totalCompra = 0;

                foreach (var item in dto.Items)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);
                    if (producto == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, $"El producto con ID {item.ProductoId} no existe.");
                    }

                    // 1. Aumentar stock en inventario
                    producto.Stock += item.Cantidad;

                    // 2. Actualizar costo unitario del producto con el nuevo precio de compra
                    if (item.CostoUnitario > 0)
                    {
                        producto.Costo = item.CostoUnitario;
                    }
                    _unitOfWork.Productos.Update(producto);

                    // 3. Registrar renglón de compra
                    var detalle = new DetalleCompra
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        CostoUnitario = item.CostoUnitario
                    };

                    totalCompra += detalle.Subtotal;
                    compra.Detalles.Add(detalle);

                    // 4. Registrar en Movimientos (Kardex de entradas)
                    var movimiento = new Movimiento
                    {
                        Fecha = DateTime.UtcNow,
                        Tipo = TipoMovimiento.Entrada,
                        Cantidad = item.Cantidad,
                        Motivo = $"Entrada por compra",
                        ProductoId = producto.Id,
                        EmpleadoId = dto.EmpleadoId
                    };
                    await _unitOfWork.Movimientos.AddAsync(movimiento);
                }

                compra.Total = totalCompra;
                await _unitOfWork.Compras.AddAsync(compra);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, "Compra registrada e inventario actualizado con éxito.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return (false, $"Error al registrar la compra: {ex.Message}");
            }
        }
    }
}