using LaTiendita.Application.DTOs;
using LaTiendita.Application.Interfaces;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Interfaces;

namespace LaTiendita.Application.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public EmpleadoService(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            return await _unitOfWork.Empleados.FindAsync(e => e.Activo);
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Empleados.GetByIdAsync(id);
        }

        public async Task<Empleado?> AutenticarAsync(string correo, string password)
        {
            return await _authService.LoginAsync(correo, password);
        }

        public async Task<bool> CrearAsync(EmpleadoDto dto)
        {
            // Validación: Correo no duplicado
            var existentes = await _unitOfWork.Empleados.FindAsync(e => e.Correo.ToLower() == dto.Correo.ToLower() && e.Activo);
            if (existentes.Any()) return false;

            var empleado = new Empleado
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Correo = dto.Correo,
                PasswordHash = _authService.HashPassword(dto.Password ?? "123456"),
                Rol = dto.Rol,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            await _unitOfWork.Empleados.AddAsync(empleado);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> ActualizarAsync(EmpleadoDto dto)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(dto.Id);
            if (empleado == null) return false;

            empleado.Nombre = dto.Nombre;
            empleado.Telefono = dto.Telefono;
            empleado.Correo = dto.Correo;
            empleado.Rol = dto.Rol;

            // Si se envió una nueva contraseña, actualizarla
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                empleado.PasswordHash = _authService.HashPassword(dto.Password);
            }

            _unitOfWork.Empleados.Update(empleado);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var empleado = await _unitOfWork.Empleados.GetByIdAsync(id);
            if (empleado == null) return false;

            // Borrado lógico para no romper integridad referencial con ventas anteriores
            empleado.Activo = false;
            _unitOfWork.Empleados.Update(empleado);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }
    }
}