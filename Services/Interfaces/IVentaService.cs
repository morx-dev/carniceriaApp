using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public interface IVentaService
{
    Task<List<Producto>> ObtenerProductosActivosAsync();
    Task<List<Cliente>> ObtenerClientesActivosAsync();
    Task<List<Usuario>> ObtenerRepartidoresActivosAsync();
    Task<List<Venta>> ObtenerPendientesAsync();
    Task<List<Venta>> ObtenerAsignadasARepartidorAsync(string repartidorId);
    Task<List<Venta>> ObtenerEntregadasPorRepartidorAsync(string repartidorId);
    Task<List<Venta>> ObtenerEntregasAsync(string? busquedaDireccion);
    Task<Venta?> ObtenerVentaConDetalleAsync(int id);

    Task<ResultadoOperacion> CrearVentaPresencialAsync(CrearVentaViewModel modelo, string usuarioId);
    Task<ResultadoOperacion> CrearVentaRemotaAsync(CrearVentaViewModel modelo, string usuarioId);
    Task<ResultadoOperacion> EditarPedidoAsync(int ventaId, List<DetalleVentaInputViewModel> detalles);
    Task<ResultadoOperacion> DespacharAsync(int ventaId, List<DetalleVentaInputViewModel> detalles);
    Task<ResultadoOperacion> AsignarRepartidorAsync(int ventaId, string repartidorId);
    Task<ResultadoOperacion> MarcarEntregadoAsync(int ventaId, string repartidorId, FormaPago formaPago, string? detalleFormaPago);
    Task<ResultadoOperacion> CancelarAsync(int ventaId);
}