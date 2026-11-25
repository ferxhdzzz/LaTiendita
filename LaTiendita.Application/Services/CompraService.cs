using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CompraService> _logger;

        public CompraService(IUnitOfWork unitOfWork, ILogger<CompraService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                    if (item.Cantidad <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, "La cantidad de cada producto debe ser mayor a cero.");
                    }

                    if (item.CostoUnitario <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, "El costo unitario de cada producto debe ser mayor a cero.");
                    }

                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);
                    if (producto == null || !producto.Activo)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, $"El producto con ID {item.ProductoId} no existe o no está activo.");
                    }

                    producto.Stock += item.Cantidad;
                    producto.Costo = item.CostoUnitario;
                    _unitOfWork.Productos.Update(producto);

                    var detalle = new DetalleCompra
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        CostoUnitario = item.CostoUnitario,
                        FechaVencimiento = item.FechaVencimiento
                    };

                    totalCompra += detalle.Subtotal;
                    compra.Detalles.Add(detalle);

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

                _logger.LogInformation("Auditoría Compra: registrada compra #{CompraId} por total {Total} con {Items} lotes (ProveedorId: {ProveedorId}, EmpleadoId: {EmpleadoId}).",
                    compra.Id, compra.Total, compra.Detalles.Count, dto.ProveedorId, dto.EmpleadoId);

                return (true, "Compra registrada e inventario actualizado con éxito.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error al registrar compra para EmpleadoId {EmpleadoId}.", dto.EmpleadoId);
                return (false, $"Error al registrar la compra: {ex.Message}");
            }
        }
    }
}