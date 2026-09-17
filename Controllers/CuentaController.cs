using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;

namespace carniceriaApp.Controllers;

public class CuentaController : Controller
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;

    public CuentaController(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager)
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

        var usuario = await _userManager.FindByEmailAsync(modelo.Email);
        if (usuario != null && !usuario.Activo)
        {
            ModelState.AddModelError(string.Empty, "Este usuario está desactivado. Contacta al administrador.");
            return View(modelo);
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            modelo.Email, modelo.Password, modelo.RecordarMe, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            return await RedirectSegunRolAsync(usuario!);
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(modelo);
    }

    private async Task<IActionResult> RedirectSegunRolAsync(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);

        if (roles.Contains("Administrador"))
            return RedirectToAction("Index", "Productos");

        // TODO: cuando exista VentasController, cambiar a RedirectToAction("Pendientes", "Ventas")
        if (roles.Contains("CallCenter"))
            return RedirectToAction("Index", "Clientes");

        // TODO: cuando exista VentasController, cambiar a RedirectToAction("Crear", "Ventas")
        if (roles.Contains("Mostrador"))
            return RedirectToAction("Index", "Clientes");

        // TODO: cuando exista VentasController, cambiar a RedirectToAction("MisEntregas", "Ventas")
        if (roles.Contains("Repartidor"))
            return RedirectToAction("Index", "Home");

        return RedirectToAction("Index", "Home");
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