using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize]
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

        var nombresCreadores = new Dictionary<string, string>();
        foreach (var venta in ventas)
        {
            if (!nombresCreadores.ContainsKey(venta.UsuarioCreadorId))
            {
                var creador = await _userManager.FindByIdAsync(venta.UsuarioCreadorId);
                nombresCreadores[venta.UsuarioCreadorId] = creador?.NombreCompleto ?? "Desconocido";
            }
        }
        ViewBag.NombresCreadores = nombresCreadores;

        return View(ventas);
    }

    // NUEVO: abre el formulario de edición (antes solo existía el POST y por eso daba 405)
    [Authorize(Roles = "Administrador,CallCenter")]
    public async Task<IActionResult> Editar(int id)
    {
        var venta = await _ventaService.ObtenerVentaConDetalleAsync(id);
        if (venta == null) return NotFound();

        if (venta.EstadoId != 1)
        {
            TempData["Error"] = "Este pedido ya no se puede editar (debe estar Pendiente).";
            return RedirectToAction(nameof(Pendientes));
        }

        ViewBag.Productos = await _ventaService.ObtenerProductosActivosAsync();
        return View(venta);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,CallCenter")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, List<DetalleVentaInputViewModel> Detalles)
    {
        var resultado = await _ventaService.EditarPedidoAsync(id, Detalles);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Pedido actualizado correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [Authorize(Roles = "Administrador,Mostrador")]
    public async Task<IActionResult> Despachar(int id)
    {
        var venta = await _ventaService.ObtenerVentaConDetalleAsync(id);
        if (venta == null) return NotFound();

        return View(venta);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Mostrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Despachar(int id, List<DetalleVentaInputViewModel> Detalles)
    {
        var resultado = await _ventaService.DespacharAsync(id, Detalles);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Pedido despachado correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,CallCenter,Mostrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarRepartidor(int id, string repartidorId)
    {
        var resultado = await _ventaService.AsignarRepartidorAsync(id, repartidorId);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Repartidor asignado correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,CallCenter,Mostrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await _ventaService.CancelarAsync(id);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Pedido cancelado." : resultado.MensajeError;

        return RedirectToAction(nameof(Pendientes));
    }

    [Authorize(Roles = "Repartidor")]
    public async Task<IActionResult> MisEntregas()
    {
        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var pendientes = await _ventaService.ObtenerAsignadasARepartidorAsync(usuarioId);
        var entregadas = await _ventaService.ObtenerEntregadasPorRepartidorAsync(usuarioId);

        ViewBag.Entregadas = entregadas;
        return View(pendientes);
    }

    [HttpPost]
    [Authorize(Roles = "Repartidor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarEntregado(int id, FormaPago formaPago, string? detalleFormaPago)
    {
        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var resultado = await _ventaService.MarcarEntregadoAsync(id, usuarioId, formaPago, detalleFormaPago);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Pedido marcado como entregado." : resultado.MensajeError;

        return RedirectToAction(nameof(MisEntregas));
    }

    [Authorize(Roles = "Administrador,CallCenter,Mostrador")]
    public async Task<IActionResult> TodasLasEntregas(string? busqueda)
    {
        var ventas = await _ventaService.ObtenerEntregasAsync(busqueda);

        var nombresRepartidores = new Dictionary<string, string>();
        var nombresCreadores = new Dictionary<string, string>();

        foreach (var venta in ventas)
        {
            if (venta.RepartidorId != null && !nombresRepartidores.ContainsKey(venta.RepartidorId))
            {
                var repartidor = await _userManager.FindByIdAsync(venta.RepartidorId);
                nombresRepartidores[venta.RepartidorId] = repartidor?.NombreCompleto ?? "Desconocido";
            }

            if (!string.IsNullOrEmpty(venta.UsuarioCreadorId) && !nombresCreadores.ContainsKey(venta.UsuarioCreadorId))
            {
                var creador = await _userManager.FindByIdAsync(venta.UsuarioCreadorId);
                nombresCreadores[venta.UsuarioCreadorId] = creador?.NombreCompleto ?? "Desconocido";
            }
        }

        ViewBag.NombresRepartidores = nombresRepartidores;
        ViewBag.NombresCreadores = nombresCreadores;
        ViewBag.BusquedaActual = busqueda;

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_ListaEntregas", ventas);
        }

        return View(ventas);
    }

    private bool EsVentaRemota(CrearVentaViewModel modelo)
    {
        if (User.IsInRole("CallCenter")) return true;
        if (User.IsInRole("Administrador") || User.IsInRole("Mostrador"))
            return modelo.TipoVenta == "Remota";
        return false;
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Productos = await _ventaService.ObtenerProductosActivosAsync();
        ViewBag.Clientes = await _ventaService.ObtenerClientesActivosAsync();
    }
}