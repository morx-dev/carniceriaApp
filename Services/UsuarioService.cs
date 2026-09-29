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

        if (!string.IsNullOrEmpty(busqueda))
        {
            usuariosQuery = usuariosQuery.Where(u =>
                (u.NombreCompleto != null && u.NombreCompleto.Contains(busqueda)) ||
                (u.Email != null && u.Email.Contains(busqueda)));
        }

        var usuarios = usuariosQuery.OrderByDescending(u => u.FechaCreacion).ToList();
        var listaConRol = new List<(Usuario Usuario, string Rol)>();

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var rolActual = roles.FirstOrDefault() ?? "Sin rol";
            listaConRol.Add((usuario, rolActual));
        }

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
        modelo.NombreCompleto = modelo.NombreCompleto?.Trim() ?? string.Empty;
        modelo.Email = modelo.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(modelo.NombreCompleto))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El nombre completo es obligatorio.", Campo = "NombreCompleto" };
        }

        var existente = await _userManager.FindByEmailAsync(modelo.Email);
        if (existente != null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Ya existe un usuario con ese correo.", Campo = "Email" };
        }

        bool rolExiste = await _roleManager.RoleExistsAsync(modelo.Rol);
        if (!rolExiste)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El rol seleccionado no es válido.", Campo = "Rol" };
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

        if (!resultado.Succeeded)
        {
            var mensajesError = string.Join(" ", resultado.Errors.Select(e => e.Description));
            return new ResultadoOperacion { Exitoso = false, MensajeError = mensajesError, Campo = "Password" };
        }

        var resultadoRol = await _userManager.AddToRoleAsync(nuevoUsuario, modelo.Rol);
        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(nuevoUsuario);
            return new ResultadoOperacion { Exitoso = false, MensajeError = "No se pudo asignar el rol al usuario." };
        }

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task<ResultadoOperacion> EditarAsync(EditarUsuarioViewModel modelo)
    {
        var usuario = await _userManager.FindByIdAsync(modelo.Id);
        if (usuario == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Usuario no encontrado." };
        }

        var nombreLimpio = modelo.NombreCompleto?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El nombre completo es obligatorio.", Campo = "NombreCompleto" };
        }

        bool rolExiste = await _roleManager.RoleExistsAsync(modelo.Rol);
        if (!rolExiste)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El rol seleccionado no es válido.", Campo = "Rol" };
        }

        usuario.NombreCompleto = nombreLimpio;
        var resultadoUpdate = await _userManager.UpdateAsync(usuario);
        if (!resultadoUpdate.Succeeded)
        {
            var mensajesError = string.Join(" ", resultadoUpdate.Errors.Select(e => e.Description));
            return new ResultadoOperacion { Exitoso = false, MensajeError = mensajesError };
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (!rolesActuales.Contains(modelo.Rol))
        {
            var resultadoQuitar = await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            if (!resultadoQuitar.Succeeded)
            {
                return new ResultadoOperacion { Exitoso = false, MensajeError = "No se pudo actualizar el rol del usuario." };
            }

            var resultadoAgregar = await _userManager.AddToRoleAsync(usuario, modelo.Rol);
            if (!resultadoAgregar.Succeeded)
            {
                return new ResultadoOperacion { Exitoso = false, MensajeError = "No se pudo asignar el nuevo rol." };
            }
        }

        return new ResultadoOperacion { Exitoso = true };
    }
    public async Task<ResultadoOperacion> CambiarEstadoAsync(string id, string usuarioActualId)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Usuario no encontrado." };
        }

        if (usuario.Id == usuarioActualId)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "No puedes cambiar el estado de tu propia cuenta." };
        }

        // Si se va a DESACTIVAR y es Administrador, verificar que no sea el único activo
        if (usuario.Activo)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            if (roles.Contains("Administrador"))
            {
                var administradores = await _userManager.GetUsersInRoleAsync("Administrador");
                var administradoresActivos = administradores.Count(a => a.Activo);

                if (administradoresActivos <= 1)
                {
                    return new ResultadoOperacion { Exitoso = false, MensajeError = "No puedes desactivar al único administrador activo del sistema." };
                }
            }
        }

        usuario.Activo = !usuario.Activo;
        var resultado = await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
        {
            var mensajesError = string.Join(" ", resultado.Errors.Select(e => e.Description));
            return new ResultadoOperacion { Exitoso = false, MensajeError = mensajesError };
        }

        return new ResultadoOperacion { Exitoso = true };
    }
}