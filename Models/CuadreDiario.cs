// Models/CuadreDiario.cs
namespace carniceriaApp.Models;

public class CuadreDiario
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public decimal TotalVentasPresenciales { get; set; }
    public decimal TotalVentasSistema { get; set; }
    public decimal TotalGeneral { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}