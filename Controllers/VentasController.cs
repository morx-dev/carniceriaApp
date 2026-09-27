using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize] // Solo exige estar logueado; el rol específico se valida en cada acción
public class VentasController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly UserManager<Usuario> _userManager;

    public VentasController(IVentaService ventaService, UserManager<Usuario> userManager)
    {
        _ventaService = ventaService;
        _userManager = userManager;
    }

    [Authorize(Roles = "Administrador,Mostrador,CallCenter")]
    public async Task<IActionResult> Crear()
    {
        await CargarListasAsync();
        return View(new CrearVentaViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Mostrador,CallCenter")]
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

    [Authorize(Roles = "Administrador,CallCenter,Mostrador")]
    public async Task<IActionResult> Pendientes()
    {
        var ventas = await _ventaService.ObtenerPendientesAsync();
        ViewBag.Repartidores = await _ventaService.ObtenerRepartidoresActivosAsync();
        return View(ventas);
    }

    [Authorize(Roles = "Administrador,CallCenter")]
    public async Task<IActionResult> Editar(int id)
    {
        var venta = await _ventaService.ObtenerVentaConDetalleAsync(id);
        if (venta == null) return NotFound();

        ViewBag.Productos = await _ventaService.ObtenerProductosActivosAsync();
        return View(venta);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,CallCenter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, List<DetalleVentaInputViewModel> Detalles, string? Observaciones)
    {
        var resultado = await _ventaService.ActualizarDetallesAsync(id, Detalles, Observaciones);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Pedido actualizado correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [Authorize(Roles = "Administrador,Mostrador")]
    public async Task<IActionResult> Despachar(int id)
    {
        var venta = await _ventaService.ObtenerVentaConDetalleAsync(id);
        if (venta == null) return NotFound();

        ViewBag.Productos = await _ventaService.ObtenerProductosActivosAsync();
        return View(venta);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Mostrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Despachar(int id, List<DetalleVentaInputViewModel> Detalles)
    {
        var resultado = await _ventaService.ActualizarDetallesAsync(id, Detalles, null);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Cantidades actualizadas correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,CallCenter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarRepartidor(int id, string repartidorId)
    {
        var resultado = await _ventaService.AsignarRepartidorAsync(id, repartidorId);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Repartidor asignado correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [Authorize(Roles = "Repartidor")]
    public async Task<IActionResult> MisEntregas()
    {
        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var ventas = await _ventaService.ObtenerAsignadasARepartidorAsync(usuarioId);
        return View(ventas);
    }

    [HttpPost]
    [Authorize(Roles = "Repartidor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarEntregado(int id)
    {
        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var resultado = await _ventaService.MarcarEntregadoAsync(id, usuarioId);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Pedido marcado como entregado." : resultado.MensajeError;

        return RedirectToAction(nameof(MisEntregas));
    }

    private bool EsVentaRemota(CrearVentaViewModel modelo)
    {
        if (User.IsInRole("Mostrador")) return false;
        if (User.IsInRole("CallCenter")) return true;
        return modelo.TipoVenta == "Remota"; // Administrador decide
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Productos = await _ventaService.ObtenerProductosActivosAsync();
        ViewBag.Clientes = await _ventaService.ObtenerClientesActivosAsync();
    }


    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> TodasLasEntregas()
    {
        var ventas = await _ventaService.ObtenerTodasEnCaminoAsync();

        var nombresRepartidores = new Dictionary<string, string>();
        foreach (var venta in ventas)
        {
            if (venta.RepartidorId != null && !nombresRepartidores.ContainsKey(venta.RepartidorId))
            {
                var repartidor = await _userManager.FindByIdAsync(venta.RepartidorId);
                nombresRepartidores[venta.RepartidorId] = repartidor?.NombreCompleto ?? "Desconocido";
            }
        }

        ViewBag.NombresRepartidores = nombresRepartidores;
        return View(ventas);
    }


}