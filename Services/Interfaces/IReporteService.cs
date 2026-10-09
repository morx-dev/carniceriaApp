using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public class ResumenCuadre
{
    public decimal TotalPresencial { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal TotalGeneral { get; set; }

    public decimal TotalAntojitosPresenciales { get; set; }
    public decimal TotalAntojitosSistema { get; set; }
    public decimal TotalAntojitosGeneral { get; set; }
}

// Resumen de UN negocio en un día
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
    Task<List<Venta>> ObtenerVentasDelDiaAsync(DateTime fecha);
    Task<List<CuadreDiario>> ObtenerHistorialAsync();
    Task<ResultadoOperacion> CerrarDiaAsync(string usuarioId, TipoNegocio negocio, DateTime? fecha = null);
}