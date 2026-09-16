using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsuariosController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(string? busqueda, string? rol)
    {
        var usuariosQuery = _userManager.Users.AsQueryable();

        // Filtrar por texto (Nombre o Correo)
        if (!string.IsNullOrEmpty(busqueda))
        {
            usuariosQuery = usuariosQuery.Where(u => u.NombreCompleto.Contains(busqueda) || u.Email.Contains(busqueda));
        }

        // Ordenar del más reciente al más antiguo según la fecha de creación
        var usuarios = usuariosQuery.OrderByDescending(u => u.FechaCreacion).ToList();
        var listaConRol = new List<(Usuario Usuario, string Rol)>();

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var rolActual = roles.FirstOrDefault() ?? "Sin rol";
            listaConRol.Add((usuario, rolActual));
        }

        // Filtrar por rol si se seleccionó uno en el select
        if (!string.IsNullOrEmpty(rol))
        {
            listaConRol = listaConRol.Where(x => x.Rol == rol).ToList();
        }

        // Guardar valores actuales para mantenerlos en la vista
        ViewBag.BusquedaActual = busqueda;
        ViewBag.RolActual = rol;
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

        // Si la petición viene por AJAX, retornamos solo la partial view de la tabla
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TablaUsuarios", listaConRol);
        }

        return View(listaConRol);
    }

    public async Task<IActionResult> Crear()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel modelo)
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();

        if (!ModelState.IsValid)
            return View(modelo);

        var existente = await _userManager.FindByEmailAsync(modelo.Email);
        if (existente != null)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un usuario con ese correo.");
            return View(modelo);
        }

        var nuevoUsuario = new Usuario
        {
            UserName = modelo.Email,
            Email = modelo.Email,
            EmailConfirmed = true,
            NombreCompleto = modelo.NombreCompleto,
            Activo = true,
            FechaCreacion = DateTime.Now // Se registra la fecha actual automáticamente
        };

        var resultado = await _userManager.CreateAsync(nuevoUsuario, modelo.Password);

        if (resultado.Succeeded)
        {
            await _userManager.AddToRoleAsync(nuevoUsuario, modelo.Rol);
            TempData["Mensaje"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in resultado.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(modelo);
    }

    public async Task<IActionResult> Editar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        var rolesActuales = await _userManager.GetRolesAsync(usuario);

        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        ViewBag.RolActual = rolesActuales.FirstOrDefault();

        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(string id, Usuario modelo, string nuevoRol)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        usuario.NombreCompleto = modelo.NombreCompleto;
        await _userManager.UpdateAsync(usuario);

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (!rolesActuales.Contains(nuevoRol))
        {
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, nuevoRol);
        }

        TempData["Mensaje"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        usuario.Activo = !usuario.Activo;
        await _userManager.UpdateAsync(usuario);

        return RedirectToAction(nameof(Index));
    }
}