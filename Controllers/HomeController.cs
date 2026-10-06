using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly ICuentaService _cuentaService;

    public HomeController(UserManager<Usuario> userManager, ICuentaService cuentaService)
    {
        _userManager = userManager;
        _cuentaService = cuentaService;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null)
        {
            return RedirectToAction("Login", "Cuenta");
        }

        var (controller, accion) = await _cuentaService.ObtenerDestinoSegunRolAsync(usuario);
        return RedirectToAction(accion, controller);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}