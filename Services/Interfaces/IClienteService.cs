using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public interface IClienteService
{
    Task<List<Cliente>> ObtenerClientesAsync(string? busqueda, int? sector);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Cliente cliente);
    Task<ResultadoOperacion> EditarAsync(int id, Cliente clienteEditado);
    Task CambiarEstadoAsync(int id);
}