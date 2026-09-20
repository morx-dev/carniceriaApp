using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador,CallCenter,Mostrador")]
public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    public async Task<IActionResult> Index(string? busqueda, int? sector)
    {
        var clientes = await _clienteService.ObtenerClientesAsync(busqueda, sector);
        
        ViewBag.BusquedaActual = busqueda;
        ViewBag.SectorActual = sector;
        ViewBag.Sectores = Enum.GetValues(typeof(SectorResidencial)).Cast<SectorResidencial>().ToList();

        // Soporte AJAX igual que Productos y Usuarios
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TablaClientes", clientes);
        }

        return View(clientes);
    }

    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cliente cliente)
    {
        if (!ModelState.IsValid)
            return View(cliente);

        await _clienteService.CrearAsync(cliente);

        TempData["Mensaje"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Cliente clienteEditado)
    {
        if (id != clienteEditado.Id) return NotFound();
        if (!ModelState.IsValid) return View(clienteEditado);

        var resultado = await _clienteService.EditarAsync(id, clienteEditado);
        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError!);
            return View(clienteEditado);
        }

        TempData["Mensaje"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        await _clienteService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index));
    }
}