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

        var cuadreCarniceria = await _reporteService.ObtenerCuadrePorFechaAsync(hoy, TipoNegocio.Carniceria);
        var cuadreAntojitos = await _reporteService.ObtenerCuadrePorFechaAsync(hoy, TipoNegocio.Antojitos);

        ViewBag.CuadreCarniceria = cuadreCarniceria;
        ViewBag.CuadreAntojitos = cuadreAntojitos;

        // Detectar días pasados sin cerrar
        ViewBag.DiasPendientesCarniceria = await _reporteService.ObtenerDiasPendientesDeCierreAsync(TipoNegocio.Carniceria);
        ViewBag.DiasPendientesAntojitos = await _reporteService.ObtenerDiasPendientesDeCierreAsync(TipoNegocio.Antojitos);

        if (cuadreCarniceria == null)
            ViewBag.ResumenCarniceria = await _reporteService.CalcularResumenDelDiaAsync(hoy, TipoNegocio.Carniceria);

        if (cuadreAntojitos == null)
            ViewBag.ResumenAntojitos = await _reporteService.CalcularResumenDelDiaAsync(hoy, TipoNegocio.Antojitos);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarDia(TipoNegocio negocio, DateTime? fecha)
    {
        // VALIDACIÓN ESTRICTA: Evita valores de enum inválidos (ej. un número aleatorio como 7)
        if (!Enum.IsDefined(typeof(TipoNegocio), negocio))
        {
            TempData["Error"] = "El tipo de negocio especificado no es válido.";
            return RedirectToAction(nameof(CuadreDiario));
        }

        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var resultado = await _reporteService.CerrarDiaAsync(usuarioId, negocio, fecha);

        var nombre = negocio == TipoNegocio.Antojitos ? "Antojitos" : "Carnicería";

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? $"El día se cerró correctamente para {nombre}." : resultado.MensajeError;

        if (fecha.HasValue && fecha.Value.Date < DateTime.Today)
        {
            return RedirectToAction(nameof(Historial), new { fecha = fecha.Value.ToString("yyyy-MM-dd") });
        }

        return RedirectToAction(nameof(CuadreDiario));
    }

    public async Task<IActionResult> Historial(DateTime? fecha)
    {
        var fechaSeleccionada = fecha?.Date ?? DateTime.Today;

        var ventas = await _reporteService.ObtenerVentasDelDiaAsync(fechaSeleccionada);

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
        ViewBag.NombresCreadores = nombresCreadores;

        var cuadreCarniceria = await _reporteService.ObtenerCuadrePorFechaAsync(fechaSeleccionada, TipoNegocio.Carniceria);
        var cuadreAntojitos = await _reporteService.ObtenerCuadrePorFechaAsync(fechaSeleccionada, TipoNegocio.Antojitos);

        ViewBag.CuadreCarniceria = cuadreCarniceria;
        ViewBag.CuadreAntojitos = cuadreAntojitos;

        // Si el día ya está cerrado, usamos los valores inmutables guardados en el cuadre
        ViewBag.ResumenCarniceria = cuadreCarniceria != null
            ? new ResumenNegocio { Presencial = cuadreCarniceria.TotalVentasPresenciales, Remoto = cuadreCarniceria.TotalVentasSistema }
            : await _reporteService.CalcularResumenDelDiaAsync(fechaSeleccionada, TipoNegocio.Carniceria);

        ViewBag.ResumenAntojitos = cuadreAntojitos != null
            ? new ResumenNegocio { Presencial = cuadreAntojitos.TotalVentasPresenciales, Remoto = cuadreAntojitos.TotalVentasSistema }
            : await _reporteService.CalcularResumenDelDiaAsync(fechaSeleccionada, TipoNegocio.Antojitos);

        return View(ventas);
    }
}