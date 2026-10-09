namespace carniceriaApp.Models;

public enum TipoOrigen { Presencial, WhatsApp, Llamada }
public enum FormaPago { Efectivo, Transferencia, Otro }
public enum TipoNegocio { Carniceria, Antojitos }

public class Venta
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public TipoOrigen TipoOrigen { get; set; }

    public int EstadoId { get; set; }
    public EstadoVenta? Estado { get; set; }

    public string UsuarioCreadorId { get; set; } = string.Empty;
    public string? RepartidorId { get; set; }

    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public FormaPago? FormaPago { get; set; }
    public string? DetalleFormaPago { get; set; }

    public bool FueEditado { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaEntrega { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();
    public TipoNegocio Negocio { get; set; } = TipoNegocio.Carniceria;
}