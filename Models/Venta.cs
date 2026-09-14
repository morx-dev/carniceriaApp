// Models/Venta.cs
namespace carniceriaApp.Models;

public enum TipoOrigen { Presencial, WhatsApp, Llamada }

public class Venta
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public TipoOrigen TipoOrigen { get; set; }

    public int EstadoId { get; set; }
    public EstadoVenta? Estado { get; set; }

    public string UsuarioCreadorId { get; set; } = string.Empty; // IdentityUser usa string como Id
    public string? RepartidorId { get; set; }

    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaEntrega { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();
}