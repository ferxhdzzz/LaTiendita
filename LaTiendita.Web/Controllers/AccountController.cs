using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LaTiendita.Infrastructure.Identity;

namespace LaTiendita.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Debes ingresar tu correo y contraseña.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(correo.Trim());
            if (user == null || !user.Activo || await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Intento de inicio de sesión web fallido para {Correo}.", correo);
                ViewBag.Error = "Correo o contraseña incorrectos, o usuario inactivo.";
                return View();
            }

            var passwordValida = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValida)
            {
                _logger.LogWarning("Contraseña incorrecta en inicio de sesión web para {Correo}.", correo);
                ViewBag.Error = "Correo o contraseña incorrectos, o usuario inactivo.";
                return View();
            }

            // Generar ClaimsPrincipal nativo de Identity (incluye roles) y anexar nombre completo y EmpleadoId
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            if (principal.Identity is ClaimsIdentity identity)
            {
                var claimNombre = identity.FindFirst(ClaimTypes.Name);
                if (claimNombre != null) identity.RemoveClaim(claimNombre);

                identity.AddClaim(new Claim(ClaimTypes.Name, user.NombreCompleto));

                if (user.EmpleadoId.HasValue)
                {
                    identity.AddClaim(new Claim("EmpleadoId", user.EmpleadoId.Value.ToString()));
                }
            }

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
            _logger.LogInformation("Inicio de sesión web exitoso para {Correo} (UserId: {UserId}).", user.Email, user.Id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var usuario = User.Identity?.Name;
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Cierre de sesión web para {Usuario}.", usuario);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}