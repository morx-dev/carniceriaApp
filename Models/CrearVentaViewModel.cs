namespace carniceriaApp.Models;

public class CrearVentaViewModel
{
    public int? ClienteId { get; set; }
    public TipoOrigen? TipoOrigenSeleccionado { get; set; }
    public string? TipoVenta { get; set; } // "Presencial" o "Remota" — solo lo usa Administrador
    public List<DetalleVentaInputViewModel> Detalles { get; set; } = new();
}

public class DetalleVentaInputViewModel
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
}