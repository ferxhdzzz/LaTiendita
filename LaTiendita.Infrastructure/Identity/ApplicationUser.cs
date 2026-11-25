using Microsoft.AspNetCore.Identity;
using LaTiendita.Domain.Entities;

namespace LaTiendita.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public int? EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }
    }
}
