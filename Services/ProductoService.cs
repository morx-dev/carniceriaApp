using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> ObtenerProductosAsync(string? busqueda, CategoriaProducto? categoria)
    {
        var query = _context.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(p => p.Nombre.Contains(busqueda));
        }

        if (categoria.HasValue)
        {
            query = query.Where(p => p.Categoria == categoria.Value);
        }

        return await query.OrderByDescending(p => p.Id).ToListAsync();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos.FindAsync(id);
    }

    public async Task<ResultadoOperacion> CrearAsync(Producto producto)
    {
        bool existeDuplicado = await _context.Productos.AnyAsync(p =>
            p.Activo &&
            p.Nombre.ToLower() == producto.Nombre.ToLower() &&
            p.Categoria == producto.Categoria &&
            p.PrecioActual == producto.PrecioActual);

        if (existeDuplicado)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                MensajeError = "Ya existe un producto activo registrado con el mismo nombre, categoría y precio."
            };
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task<ResultadoOperacion> EditarAsync(int id, Producto productoEditado, string usuarioId)
    {
        bool existeDuplicado = await _context.Productos.AnyAsync(p =>
            p.Id != id &&
            p.Activo &&
            p.Nombre.ToLower() == productoEditado.Nombre.ToLower() &&
            p.Categoria == productoEditado.Categoria &&
            p.PrecioActual == productoEditado.PrecioActual);

        if (existeDuplicado)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                MensajeError = "Ya existe otro producto activo con el mismo nombre, categoría y precio."
            };
        }

        var productoActual = await _context.Productos.FindAsync(id);
        if (productoActual == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Producto no encontrado." };
        }

        if (productoActual.PrecioActual != productoEditado.PrecioActual)
        {
            _context.HistorialPrecios.Add(new HistorialPrecio
            {
                ProductoId = productoActual.Id,
                PrecioAnterior = productoActual.PrecioActual!.Value,
                PrecioNuevo = productoEditado.PrecioActual!.Value,
                UsuarioId = usuarioId
            });
        }

        productoActual.Nombre = productoEditado.Nombre;
        productoActual.Categoria = productoEditado.Categoria;
        productoActual.PrecioActual = productoEditado.PrecioActual;
        productoActual.Activo = productoEditado.Activo;

        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task DesactivarAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            producto.Activo = false;
            await _context.SaveChangesAsync();
        }
    }
}