using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador,Mostrador,CallCenter")]
public class VentasController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly UserManager<Usuario> _userManager;

    public VentasController(IVentaService ventaService, UserManager<Usuario> userManager)
    {
        _ventaService = ventaService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Crear()
    {
        await CargarListasAsync();
        return View(new CrearVentaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearVentaViewModel modelo)
    {
        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        bool esRemota = EsVentaRemota(modelo);

        var resultado = esRemota
            ? await _ventaService.CrearVentaRemotaAsync(modelo, usuarioId)
            : await _ventaService.CrearVentaPresencialAsync(modelo, usuarioId);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError!);
            await CargarListasAsync();
            return View(modelo);
        }

        TempData["Mensaje"] = "Venta registrada correctamente.";
        return RedirectToAction(nameof(Crear));
    }

    private bool EsVentaRemota(CrearVentaViewModel modelo)
    {
        if (User.IsInRole("Administrador"))
            return modelo.TipoVenta == "Remota";

        if (User.IsInRole("CallCenter"))
            return true;

        return false; // Mostrador
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Productos = await _ventaService.ObtenerProductosActivosAsync();
        ViewBag.Clientes = await _ventaService.ObtenerClientesActivosAsync();
    }
}