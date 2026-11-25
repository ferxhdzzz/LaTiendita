using System.ComponentModel.DataAnnotations;
using LaTiendita.Domain.Enums;

namespace LaTiendita.Application.DTOs
{
    public class EmpleadoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "El teléfono debe tener entre 8 y 15 caracteres.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido.")]
        [StringLength(60, ErrorMessage = "El correo no puede exceder los 60 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(30, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 30 caracteres.")]
        public string? Password { get; set; } // Opcional al editar

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public RolUsuario Rol { get; set; } = RolUsuario.Empleado;
    }
}