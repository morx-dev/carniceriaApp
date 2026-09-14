// Models/DetalleVenta.cs
namespace carniceriaApp.Models;

public class DetalleVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}