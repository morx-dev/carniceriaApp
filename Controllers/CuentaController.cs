using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;

namespace carniceriaApp.Controllers;

public class CuentaController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public CuentaController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _signInManager.PasswordSignInAsync(
            modelo.Email, modelo.Password, modelo.RecordarMe, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Obtenemos el usuario para evaluar sus roles y redirigirlo a su módulo principal
            var user = await _userManager.FindByEmailAsync(modelo.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Administrador"))
                    return RedirectToAction("Index", "Productos");

                if (roles.Contains("CallCenter"))
                    return RedirectToAction("Pendientes", "Ventas");

                if (roles.Contains("Mostrador"))
                    return RedirectToAction("Crear", "Ventas");

                if (roles.Contains("Repartidor"))
                    return RedirectToAction("MisEntregas", "Ventas");
            }

            return RedirectToAction("Index", "Productos");
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Cuenta");
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}