using LaTiendita.Domain.Entities;
using LaTiendita.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LaTiendita.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                context.Database.Migrate();
            }
            catch
            {
                context.Database.EnsureCreated();
                try
                {
                    context.Database.ExecuteSqlRaw(
                        "IF COL_LENGTH('DetallesCompra', 'FechaVencimiento') IS NULL ALTER TABLE DetallesCompra ADD FechaVencimiento datetime2 NULL;");
                }
                catch
                {
                    // Ignorar si la tabla ya cuenta con la columna
                }
            }

            // Si no hay empleados, crear el Administrador por defecto
            if (!context.Empleados.Any())
            {
                var admin = new Empleado
                {
                    Nombre = "Administrador Principal",
                    Telefono = "2222-4510",
                    Correo = "admin@tiendita.com",
                    PasswordHash = HashPassword("admin123"),
                    Rol = RolUsuario.Administrador,
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                };
                context.Empleados.Add(admin);
            }

            // Categorías base
            if (!context.Categorias.Any())
            {
                context.Categorias.AddRange(
                    new Categoria { Nombre = "Bebidas", Descripcion = "Refrescos, jugos y aguas" },
                    new Categoria { Nombre = "Snacks", Descripcion = "Frituras y botanas" },
                    new Categoria { Nombre = "Abarrotes", Descripcion = "Granos, pastas y enlatados" }
                );
            }

            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}