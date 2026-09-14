// Models/Cliente.cs
namespace carniceriaApp.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Referencia { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}