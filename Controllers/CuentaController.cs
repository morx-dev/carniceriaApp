using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

public class CuentaController : Controller
{
    private readonly ICuentaService _cuentaService;

    public CuentaController(ICuentaService cuentaService)
    {
        _cuentaService = cuentaService;
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

        var resultado = await _cuentaService.IniciarSesionAsync(modelo.Email, modelo.Password, modelo.RecordarMe);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError!);
            return View(modelo);
        }

        return RedirectToAction(resultado.AccionDestino!, resultado.ControllerDestino!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _cuentaService.CerrarSesionAsync();
        return RedirectToAction("Login", "Cuenta");
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}