using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportesController : Controller
{
    private readonly IReporteService _reporteService;
    private readonly UserManager<Usuario> _userManager;

    public ReportesController(IReporteService reporteService, UserManager<Usuario> userManager)
    {
        _reporteService = reporteService;
        _userManager = userManager;
    }

    public async Task<IActionResult> CuadreDiario()
    {
        var hoy = DateTime.Today;
        var cuadreDeHoy = await _reporteService.ObtenerCuadrePorFechaAsync(hoy);

        if (cuadreDeHoy == null)
        {
            ViewBag.Resumen = await _reporteService.CalcularResumenDelDiaAsync(hoy);
        }

        return View(cuadreDeHoy);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarDia()
    {
        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var resultado = await _reporteService.CerrarDiaAsync(usuarioId);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "El día se cerró correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(CuadreDiario));
    }

    public async Task<IActionResult> Historial(DateTime? fecha)
    {
        var fechaSeleccionada = fecha?.Date ?? DateTime.Today;

        var ventas = await _reporteService.ObtenerVentasDelDiaAsync(fechaSeleccionada);
        var cuadre = await _reporteService.ObtenerCuadrePorFechaAsync(fechaSeleccionada);

        var nombresCreadores = new Dictionary<string, string>();
        foreach (var venta in ventas)
        {
            if (!nombresCreadores.ContainsKey(venta.UsuarioCreadorId))
            {
                var creador = await _userManager.FindByIdAsync(venta.UsuarioCreadorId);
                nombresCreadores[venta.UsuarioCreadorId] = creador?.NombreCompleto ?? "Desconocido";
            }
        }

        ViewBag.FechaSeleccionada = fechaSeleccionada;
        ViewBag.Cuadre = cuadre;
        ViewBag.NombresCreadores = nombresCreadores;

        return View(ventas);
    }
}