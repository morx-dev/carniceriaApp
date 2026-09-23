using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public interface IVentaService
{
    Task<List<Producto>> ObtenerProductosActivosAsync();
    Task<List<Cliente>> ObtenerClientesActivosAsync();
    Task<ResultadoOperacion> CrearVentaPresencialAsync(CrearVentaViewModel modelo, string usuarioId);
    Task<ResultadoOperacion> CrearVentaRemotaAsync(CrearVentaViewModel modelo, string usuarioId);
}