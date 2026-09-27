using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public interface IVentaService
{
    Task<List<Producto>> ObtenerProductosActivosAsync();
    Task<List<Cliente>> ObtenerClientesActivosAsync();
    Task<List<Usuario>> ObtenerRepartidoresActivosAsync();
    Task<List<Venta>> ObtenerPendientesAsync();
    Task<List<Venta>> ObtenerAsignadasARepartidorAsync(string repartidorId);
    Task<List<Venta>> ObtenerTodasEnCaminoAsync();
    Task<Venta?> ObtenerVentaConDetalleAsync(int id);

    Task<ResultadoOperacion> CrearVentaPresencialAsync(CrearVentaViewModel modelo, string usuarioId);
    Task<ResultadoOperacion> CrearVentaRemotaAsync(CrearVentaViewModel modelo, string usuarioId);
    Task<ResultadoOperacion> ActualizarDetallesAsync(int ventaId, List<DetalleVentaInputViewModel> detalles, string? observaciones);
    Task<ResultadoOperacion> AsignarRepartidorAsync(int ventaId, string repartidorId);
    Task<ResultadoOperacion> MarcarEntregadoAsync(int ventaId, string repartidorId);
}