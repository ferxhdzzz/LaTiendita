using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Enums;
using LaTiendita.Infrastructure.Identity;

namespace LaTiendita.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplicar migraciones pendientes de EF Core (incluyendo tablas de Identity)
            await context.Database.MigrateAsync();

            // 1. Crear roles base en ASP.NET Core Identity
            string[] roles = { nameof(RolUsuario.Administrador), nameof(RolUsuario.Empleado) };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Crear registro base de empleado y usuario administrador en Identity
            const string adminEmail = "admin@tiendita.com";
            var adminEmpleado = await context.Empleados
                .FirstOrDefaultAsync(e => e.Correo.ToLower() == adminEmail);

            if (adminEmpleado == null)
            {
                adminEmpleado = new Empleado
                {
                    Nombre = "Administrador Principal",
                    Telefono = "2222-4510",
                    Correo = adminEmail,
                    PasswordHash = "IDENTITY_MANAGED",
                    Rol = RolUsuario.Administrador,
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                };
                context.Empleados.Add(adminEmpleado);
                await context.SaveChangesAsync();
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = adminEmpleado.Telefono,
                    NombreCompleto = adminEmpleado.Nombre,
                    Activo = true,
                    EmpleadoId = adminEmpleado.Id
                };

                var resultado = await userManager.CreateAsync(adminUser, "admin123");
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, nameof(RolUsuario.Administrador));
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, nameof(RolUsuario.Administrador)))
            {
                await userManager.AddToRoleAsync(adminUser, nameof(RolUsuario.Administrador));
            }

            // 3. Categorías base
            if (!await context.Categorias.AnyAsync())
            {
                context.Categorias.AddRange(
                    new Categoria { Nombre = "Bebidas", Descripcion = "Refrescos, jugos y aguas" },
                    new Categoria { Nombre = "Snacks", Descripcion = "Frituras y botanas" },
                    new Categoria { Nombre = "Abarrotes", Descripcion = "Granos, pastas y enlatados" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}