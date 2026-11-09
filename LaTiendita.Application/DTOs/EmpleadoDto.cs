using LaTiendita.Domain.Enums;

namespace LaTiendita.Application.DTOs
{
    public class EmpleadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Password { get; set; } // Opcional al editar
        public RolUsuario Rol { get; set; } = RolUsuario.Empleado;
    }
}