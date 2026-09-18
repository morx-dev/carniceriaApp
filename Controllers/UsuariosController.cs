using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index(string? busqueda, string? rol)
    {
        var listaConRol = await _usuarioService.ObtenerUsuariosAsync(busqueda, rol);

        ViewBag.BusquedaActual = busqueda;
        ViewBag.RolActual = rol;
        ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TablaUsuarios", listaConRol);
        }

        return View(listaConRol);
    }

    public async Task<IActionResult> Crear()
    {
        ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel modelo)
    {
        ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesAsync();

        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _usuarioService.CrearAsync(modelo);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError!);
            return View(modelo);
        }

        TempData["Mensaje"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(string id)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id);
        if (usuario == null) return NotFound();

        var rolActual = await _usuarioService.ObtenerRolActualAsync(usuario);

        var modelo = new EditarUsuarioViewModel
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            Rol = rolActual ?? string.Empty
        };

        ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesAsync();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarUsuarioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesAsync();
            return View(modelo);
        }

        await _usuarioService.EditarAsync(modelo);

        TempData["Mensaje"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id)
    {
        await _usuarioService.CambiarEstadoAsync(id);
        return RedirectToAction(nameof(Index));
    }
}