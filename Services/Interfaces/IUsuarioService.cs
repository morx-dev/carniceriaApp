using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public interface IUsuarioService
{
    Task<List<(Usuario Usuario, string Rol)>> ObtenerUsuariosAsync(string? busqueda, string? rol);
    Task<List<string?>> ObtenerRolesDisponiblesAsync();
    Task<Usuario?> ObtenerPorIdAsync(string id);
    Task<string?> ObtenerRolActualAsync(Usuario usuario);
    Task<ResultadoOperacion> CrearAsync(CrearUsuarioViewModel modelo);
    Task EditarAsync(EditarUsuarioViewModel modelo);
    Task CambiarEstadoAsync(string id);
}