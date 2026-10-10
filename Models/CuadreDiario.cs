// Models/CuadreDiario.cs
namespace carniceriaApp.Models;

public class CuadreDiario
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string UsuarioId { get; set; } = string.Empty;

    // Negocio al que pertenece este cuadre (una fila por día y por negocio: Carnicería o Antojitos)
    public TipoNegocio Negocio { get; set; } = TipoNegocio.Carniceria;

    // Totales específicos del negocio de esta fila
    public decimal TotalVentasPresenciales { get; set; }
    public decimal TotalVentasSistema { get; set; } // Ventas remotas/sistema entregadas de este negocio
    public decimal TotalGeneral { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}