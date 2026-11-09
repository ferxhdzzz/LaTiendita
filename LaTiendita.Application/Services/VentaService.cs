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

        public VentaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            // 1. Iniciar Transacción Atómica
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
                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);
                    if (producto == null || !producto.Activo)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, $"El producto con ID {item.ProductoId} no existe o no está activo.");
                    }

                    // Validación de Regla de Negocio: Stock suficiente
                    if (producto.Stock < item.Cantidad)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (false, $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}.");
                    }

                    // Descontar inventario
                    producto.Stock -= item.Cantidad;
                    _unitOfWork.Productos.Update(producto);

                    // Registrar renglón de detalle
                    var detalle = new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario > 0 ? item.PrecioUnitario : producto.Precio,
                        CostoUnitario = producto.Costo // Guardar costo histórico para cálculo de utilidades
                    };

                    totalVenta += detalle.Subtotal;
                    venta.Detalles.Add(detalle);

                    // Registrar auditoría en Movimientos (Kardex)
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

                // Guardar todos los cambios
                await _unitOfWork.SaveChangesAsync();

                // 2. Confirmar transacción
                await _unitOfWork.CommitTransactionAsync();

                return (true, "Venta registrada con éxito.");
            }
            catch (Exception ex)
            {
                // Revertir todo en caso de error imprevisto
                await _unitOfWork.RollbackTransactionAsync();
                return (false, $"Error al procesar la venta: {ex.Message}");
            }
        }
    }
}