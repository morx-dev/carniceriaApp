using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public class ResultadoOperacion
{
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }
}

public interface IProductoService
{
    Task<List<Producto>> ObtenerProductosAsync(string? busqueda, CategoriaProducto? categoria);
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion> CrearAsync(Producto producto);
    Task<ResultadoOperacion> EditarAsync(int id, Producto productoEditado, string usuarioId);
    Task DesactivarAsync(int id);
}