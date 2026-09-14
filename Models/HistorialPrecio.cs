// Models/HistorialPrecio.cs
namespace carniceriaApp.Models;

public class HistorialPrecio
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal PrecioAnterior { get; set; }
    public decimal PrecioNuevo { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.Now;
    public string UsuarioId { get; set; } = string.Empty;
}