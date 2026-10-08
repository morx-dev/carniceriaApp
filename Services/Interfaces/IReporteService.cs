using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public class ResumenCuadre
{
    public decimal TotalPresencial { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal TotalGeneral { get; set; }
}

public interface IReporteService
{
    Task<CuadreDiario?> ObtenerCuadrePorFechaAsync(DateTime fecha);
    Task<ResumenCuadre> CalcularResumenDelDiaAsync(DateTime fecha);
    Task<List<Venta>> ObtenerVentasDelDiaAsync(DateTime fecha);
    Task<List<CuadreDiario>> ObtenerHistorialAsync();
    Task<ResultadoOperacion> CerrarDiaAsync(string usuarioId);
}