using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public class ResumenNegocio
{
    public decimal Presencial { get; set; }
    public decimal Remoto { get; set; }
    public decimal Total => Presencial + Remoto;
}

public interface IReporteService
{
    Task<CuadreDiario?> ObtenerCuadrePorFechaAsync(DateTime fecha, TipoNegocio negocio);
    Task<ResumenNegocio> CalcularResumenDelDiaAsync(DateTime fecha, TipoNegocio negocio);
    Task<List<CuadreDiario>> ObtenerHistorialAsync();
    Task<List<Venta>> ObtenerVentasDelDiaAsync(DateTime fecha);
    Task<ResultadoOperacion> CerrarDiaAsync(string usuarioId, TipoNegocio negocio, DateTime? fecha = null);
    Task<List<DateTime>> ObtenerDiasPendientesDeCierreAsync(TipoNegocio negocio);
}