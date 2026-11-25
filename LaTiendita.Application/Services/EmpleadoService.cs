using Microsoft.Extensions.Logging;
using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmpleadoService> _logger;

        public EmpleadoService(IUnitOfWork unitOfWork, ILogger<EmpleadoService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            return await _unitOfWork.Empleados.FindAsync(e => e.Activo);
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Empleados.GetByIdAsync(id);
        }

        public async Task<Empleado?> CrearAsync(EmpleadoDto dto)
        {
            var existentes = await _unitOfWork.Empleados.FindAsync(e => e.Correo.ToLower() == dto.Correo.ToLower() && e.Activo);
            if (existentes.Any()) return null;

            var empleado = new Empleado
            {
                Nombre = dto.Nombre.Trim(),
                Telefono = dto.Telefono.Trim(),
                Correo = dto.Correo.Trim(),
                PasswordHash = "IDENTITY_MANAGED",
                Rol = dto.Rol,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            await _unitOfWork.Empleados.AddAsync(empleado);
            var guardado = await _unitOfWork.SaveChangesAsync() > 0;
            if (!guardado) return null;

            _logger.LogInformation("Auditoría Empleado: registrado empleado {EmpleadoId} ({Correo}) con rol {Rol}.",
                empleado.Id, empleado.Correo, empleado.Rol);

            return empleado;
        }

        public async Task<bool> ActualizarAsync(EmpleadoDto dto)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(dto.Id);
            if (empleado == null) return false;

            var duplicados = await _unitOfWork.Empleados.FindAsync(e =>
                e.Id != dto.Id && e.Correo.ToLower() == dto.Correo.ToLower() && e.Activo);
            if (duplicados.Any()) return false;

            empleado.Nombre = dto.Nombre.Trim();
            empleado.Telefono = dto.Telefono.Trim();
            empleado.Correo = dto.Correo.Trim();
            empleado.Rol = dto.Rol;

            _unitOfWork.Empleados.Update(empleado);
            var actualizado = await _unitOfWork.SaveChangesAsync() > 0;

            if (actualizado)
            {
                _logger.LogInformation("Auditoría Empleado: actualizado empleado {EmpleadoId} ({Correo}) con rol {Rol}.",
                    empleado.Id, empleado.Correo, empleado.Rol);
            }

            return actualizado;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null) return false;

            empleado.Activo = false;
            _unitOfWork.Empleados.Update(empleado);
            var eliminado = await _unitOfWork.SaveChangesAsync() > 0;

            if (eliminado)
            {
                _logger.LogInformation("Auditoría Empleado: desactivado empleado {EmpleadoId} ({Correo}).",
                    empleado.Id, empleado.Correo);
            }

            return eliminado;
        }
    }
}