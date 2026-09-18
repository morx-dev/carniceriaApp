using Microsoft.AspNetCore.Identity;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class UsuarioService : IUsuarioService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsuarioService(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<List<(Usuario Usuario, string Rol)>> ObtenerUsuariosAsync(string? busqueda, string? rol)
    {
        var usuariosQuery = _userManager.Users.AsQueryable();

        // Filtrar por texto (Nombre o Correo) de forma segura contra nulos
        if (!string.IsNullOrEmpty(busqueda))
        {
            usuariosQuery = usuariosQuery.Where(u => 
                (u.NombreCompleto != null && u.NombreCompleto.Contains(busqueda)) || 
                (u.Email != null && u.Email.Contains(busqueda)));
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

        return listaConRol;
    }

    public Task<List<string?>> ObtenerRolesDisponiblesAsync()
    {
        return Task.FromResult(_roleManager.Roles.Select(r => r.Name).ToList());
    }

    public async Task<Usuario?> ObtenerPorIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<string?> ObtenerRolActualAsync(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        return roles.FirstOrDefault();
    }

    public async Task<ResultadoOperacion> CrearAsync(CrearUsuarioViewModel modelo)
    {
        var existente = await _userManager.FindByEmailAsync(modelo.Email);
        if (existente != null)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                MensajeError = "Ya existe un usuario con ese correo."
            };
        }

        var nuevoUsuario = new Usuario
        {
            UserName = modelo.Email,
            Email = modelo.Email,
            EmailConfirmed = true,
            NombreCompleto = modelo.NombreCompleto,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        var resultado = await _userManager.CreateAsync(nuevoUsuario, modelo.Password);

        if (resultado.Succeeded)
        {
            await _userManager.AddToRoleAsync(nuevoUsuario, modelo.Rol);
            return new ResultadoOperacion { Exitoso = true };
        }

        var mensajesError = string.Join(" ", resultado.Errors.Select(e => e.Description));
        return new ResultadoOperacion { Exitoso = false, MensajeError = mensajesError };
    }

    public async Task EditarAsync(EditarUsuarioViewModel modelo)
    {
        var usuario = await _userManager.FindByIdAsync(modelo.Id);
        if (usuario == null) return;

        usuario.NombreCompleto = modelo.NombreCompleto;
        await _userManager.UpdateAsync(usuario);

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (!rolesActuales.Contains(modelo.Rol))
        {
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, modelo.Rol);
        }
    }

    public async Task CambiarEstadoAsync(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return;

        usuario.Activo = !usuario.Activo;
        await _userManager.UpdateAsync(usuario);
    }
}