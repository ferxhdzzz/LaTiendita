using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LaTiendita.Application.Interfaces;
using LaTiendita.Application.Services;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;
using LaTiendita.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. BASE DE DATOS SQL SERVER
// =========================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================================================
// 2. UNIDAD DE TRABAJO Y REPOSITORIOS (DIP / SOLID)
// =========================================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =========================================================
// 3. SERVICIOS DE NEGOCIO Y SERVICIO JWT
// =========================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// Inyección de JwtService con lectura segura de appsettings.json
builder.Services.AddScoped<IJwtService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var secretKey = config["JwtSettings:SecretKey"] ?? "LaTiendita_SuperSecretKey_2026_DesarrolloWebSeguro_TokenKeyJWT!";
    var issuer = config["JwtSettings:Issuer"] ?? "LaTienditaApi";
    var audience = config["JwtSettings:Audience"] ?? "LaTienditaClients";
    var duration = int.Parse(config["JwtSettings:DurationInHours"] ?? "8");
    return new JwtService(secretKey, issuer, audience, duration);
});

// =========================================================
// 4. AUTENTICACIÓN DUAL (COOKIES PARA WEB + JWT PARA API)
// =========================================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["SecretKey"] ?? "LaTiendita_SuperSecretKey_2026_DesarrolloWebSeguro_TokenKeyJWT!";

builder.Services.AddAuthentication(options =>
{
    // Por defecto en navegador usa cookies
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "LaTienditaApi",
        ValidAudience = jwtSettings["Audience"] ?? "LaTienditaClients",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// =========================================================
// 5. POLÍTICAS DE AUTORIZACIÓN (Módulo 2.4)
// =========================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", policy =>
        policy.RequireRole("Administrador"));

    options.AddPolicy("PersonalTienda", policy =>
        policy.RequireRole("Administrador", "Empleado"));
});

// =========================================================
// 6. PROTECCIÓN CSRF / ANTIFORGERY (Módulo 3.1)
// =========================================================
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================================================
// 7. INICIALIZAR BASE DE DATOS AL ARRANCAR
// =========================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Initialize(context);
}

// Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 8. MIDDLEWARE DE SEGURIDAD
app.UseAuthentication();
app.UseAuthorization();

// 9. RUTAS MVC + RUTAS DE CONTROLADORES API REST
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers(); // Habilita las rutas de la API (/api/...)

app.Run();