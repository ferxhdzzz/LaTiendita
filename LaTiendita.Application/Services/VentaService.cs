using Microsoft.Extensions.Logging;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Enums;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class VentaService : IVentaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VentaService> _logger;

        public VentaService(IUnitOfWork unitOfWork, ILogger<VentaService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Venta>> GetAllAsync()
        {
            return await _unitOfWork.Ventas.GetVentasConDetallesAsync();
        }

        public async Task<Venta?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Ventas.GetVentaConDetallesAsync(id);
        }

        public async Task<(bool Exito, string Mensaje)> RegistrarVentaAsync(VentaRegistroDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return (false, "La venta debe incluir al menos un producto.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var venta = new Venta
                {
                    Fecha = DateTime.UtcNow,
                    EmpleadoId = dto.EmpleadoId,
                    Total = 0
                };

                decimal totalVenta = 0;

                foreach (var item in dto.Items)
                {
                    if (item.Cantidad <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, "La cantidad de cada producto debe ser mayor a cero.");
                    }

                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);
                    if (producto == null || !producto.Activo)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, $"El producto con ID {item.ProductoId} no existe o no está activo.");
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}.");
                    }

                    producto.Stock -= item.Cantidad;
                    _unitOfWork.Productos.Update(producto);

                    var detalle = new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        CostoUnitario = producto.Costo
                    };

                    totalVenta += detalle.Subtotal;
                    venta.Detalles.Add(detalle);

                    var movimiento = new Movimiento
                    {
                        Fecha = DateTime.UtcNow,
                        Tipo = TipoMovimiento.Salida,
                        Cantidad = item.Cantidad,
                        Motivo = $"Venta de mostrador",
                        ProductoId = producto.Id,
                        EmpleadoId = dto.EmpleadoId
                    };
                    await _unitOfWork.Movimientos.AddAsync(movimiento);
                }

                venta.Total = totalVenta;
                await _unitOfWork.Ventas.AddAsync(venta);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Auditoría Venta: registrada venta #{VentaId} por total {Total} con {Items} productos (EmpleadoId: {EmpleadoId}).",
                    venta.Id, venta.Total, venta.Detalles.Count, dto.EmpleadoId);

                return (true, "Venta registrada con éxito.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error al procesar venta para EmpleadoId {EmpleadoId}.", dto.EmpleadoId);
                return (false, $"Error al procesar la venta: {ex.Message}");
            }
        }
    }
}