using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly UserManager<Usuario> _userManager;

    public UsuariosController(IUsuarioService usuarioService, UserManager<Usuario> userManager)
    {
        _usuarioService = usuarioService;
        _userManager = userManager;
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
            if (!string.IsNullOrEmpty(resultado.Campo))
                ModelState.AddModelError(resultado.Campo, resultado.MensajeError!);
            else
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

        var resultado = await _usuarioService.EditarAsync(modelo);

        if (!resultado.Exitoso)
        {
            if (!string.IsNullOrEmpty(resultado.Campo))
                ModelState.AddModelError(resultado.Campo, resultado.MensajeError!);
            else
                ModelState.AddModelError(string.Empty, resultado.MensajeError!);

            ViewBag.Roles = await _usuarioService.ObtenerRolesDisponiblesAsync();
            return View(modelo);
        }

        TempData["Mensaje"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id)
    {
        var usuarioActualId = _userManager.GetUserId(User) ?? string.Empty;
        var resultado = await _usuarioService.CambiarEstadoAsync(id, usuarioActualId);

        TempData[resultado.Exitoso ? "Mensaje" : "Error"] =
            resultado.Exitoso ? "Estado actualizado correctamente." : resultado.MensajeError;

        return RedirectToAction(nameof(Index));
    }
}