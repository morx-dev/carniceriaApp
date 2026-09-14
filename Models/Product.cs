// Models/Producto.cs
namespace carniceriaApp.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Categoria { get; set; }
    public string UnidadMedida { get; set; } = "Libra";
    public decimal PrecioActual { get; set; }
    public bool Activo { get; set; } = true;
}