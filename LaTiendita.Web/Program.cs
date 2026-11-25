using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using LaTiendita.Application.Interfaces;
using LaTiendita.Application.Services;
using LaTiendita.Domain.Interfaces;
using LaTiendita.Infrastructure.Data;
using LaTiendita.Infrastructure.Identity;
using LaTiendita.Infrastructure.Repositories;
using LaTiendita.Web.Middleware;

// Configurar Serilog (consola + archivos diarios de log)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(Directory.GetCurrentDirectory(), "Logs", "latiendita-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // Base de datos SQL Server
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "No se encontró 'ConnectionStrings:DefaultConnection'. " +
            "Configúrala localmente con 'dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\"' " +
            "o mediante la variable de entorno ConnectionStrings__DefaultConnection.");
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    // ASP.NET Core Identity (usuarios, roles y política práctica de contraseñas)
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Cookie de sesión de Identity para MVC (y 401/403 JSON limpio para /api)
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

    // Repositorios y unidad de trabajo
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Servicios de aplicación
    builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
    builder.Services.AddScoped<IProductoService, ProductoService>();
    builder.Services.AddScoped<IVentaService, VentaService>();
    builder.Services.AddScoped<ICompraService, CompraService>();
    builder.Services.AddScoped<IReporteService, ReporteService>();

    // Configuración JWT desde secretos de usuario o variables de entorno
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var jwtKey = jwtSettings["SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    {
        throw new InvalidOperationException(
            "No se encontró una clave válida en 'JwtSettings:SecretKey' (mínimo 32 caracteres). " +
            "Configúrala localmente con 'dotnet user-secrets set \"JwtSettings:SecretKey\" \"...\"' " +
            "o mediante la variable de entorno JwtSettings__SecretKey.");
    }

    var jwtIssuer = jwtSettings["Issuer"] ?? "LaTienditaApi";
    var jwtAudience = jwtSettings["Audience"] ?? "LaTienditaClients";
    var jwtDuration = int.TryParse(jwtSettings["DurationInHours"], out int horas) && horas > 0 ? horas : 8;

    builder.Services.AddScoped<IJwtService>(_ =>
        new JwtService(jwtKey, jwtIssuer, jwtAudience, jwtDuration));

    // Esquema Bearer JWT para la API REST (sin sobrescribir el esquema por defecto de Identity)
    builder.Services.AddAuthentication()
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

    // Políticas de autorización por rol
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SoloAdmin", policy =>
            policy.RequireRole("Administrador"));

        options.AddPolicy("PersonalTienda", policy =>
            policy.RequireRole("Administrador", "Empleado"));
    });

    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-CSRF-TOKEN";
    });

    builder.Services.AddControllersWithViews();

    // Documentación OpenAPI / Swagger con comentarios XML y autenticación Bearer
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "LaTiendita API",
            Version = "v1",
            Description = "API RESTful de autenticación JWT y gestión de productos para La Tiendita."
        });

        var xmlWeb = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        if (File.Exists(xmlWeb))
        {
            options.IncludeXmlComments(xmlWeb);
        }

        var xmlApp = Path.Combine(AppContext.BaseDirectory, "LaTiendita.Application.xml");
        if (File.Exists(xmlApp))
        {
            options.IncludeXmlComments(xmlApp);
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingresa el token JWT obtenido en POST /api/auth/login"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });
    });

    var app = builder.Build();

    // Migraciones y datos base (Identity roles, administrador y categorías)
    using (var scope = app.Services.CreateScope())
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);
    }

    app.UseSerilogRequestLogging();
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "LaTiendita API v1");
            options.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapControllers();

    Log.Information("Aplicación LaTiendita iniciada correctamente.");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "La aplicación LaTiendita se detuvo inesperadamente.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}