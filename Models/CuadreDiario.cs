// Models/CuadreDiario.cs
namespace carniceriaApp.Models;

public class CuadreDiario
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string UsuarioId { get; set; } = string.Empty;

    // Negocio al que pertenece este cuadre (una fila por día y por negocio)
    public TipoNegocio Negocio { get; set; } = TipoNegocio.Carniceria;

    // Totales del negocio de esta fila
    public decimal TotalVentasPresenciales { get; set; }
    public decimal TotalVentasSistema { get; set; }
    public decimal TotalGeneral { get; set; }

    // Campos antiguos: se conservan por compatibilidad. Los cuadres nuevos los dejan en 0.
    public decimal TotalAntojitosPresenciales { get; set; }
    public decimal TotalAntojitosSistema { get; set; }
    public decimal TotalAntojitosGeneral { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}