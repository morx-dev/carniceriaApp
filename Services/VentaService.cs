using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> ObtenerProductosActivosAsync()
    {
        return await _context.Productos
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Cliente>> ObtenerClientesActivosAsync()
    {
        return await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<ResultadoOperacion> CrearVentaPresencialAsync(CrearVentaViewModel modelo, string usuarioId)
    {
        var venta = new Venta
        {
            ClienteId = modelo.ClienteId,
            TipoOrigen = TipoOrigen.Presencial,
            EstadoId = 4, // Entregado — inmediato para venta de mostrador
            UsuarioCreadorId = usuarioId,
            FechaCreacion = DateTime.Now,
            FechaEntrega = DateTime.Now
        };

        return await ArmarYGuardarVentaAsync(venta, modelo.Detalles);
    }

    public async Task<ResultadoOperacion> CrearVentaRemotaAsync(CrearVentaViewModel modelo, string usuarioId)
    {
        if (modelo.ClienteId == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Debes seleccionar un cliente." };
        }

        var venta = new Venta
        {
            ClienteId = modelo.ClienteId,
            TipoOrigen = modelo.TipoOrigenSeleccionado ?? TipoOrigen.WhatsApp,
            EstadoId = 1, // Pendiente — a la espera de asignar repartidor
            UsuarioCreadorId = usuarioId,
            FechaCreacion = DateTime.Now
        };

        return await ArmarYGuardarVentaAsync(venta, modelo.Detalles);
    }

    private async Task<ResultadoOperacion> ArmarYGuardarVentaAsync(Venta venta, List<DetalleVentaInputViewModel> detalles)
    {
        if (detalles == null || !detalles.Any(d => d.Cantidad > 0))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Debes agregar al menos un producto a la venta." };
        }

        decimal total = 0;

        foreach (var item in detalles.Where(d => d.Cantidad > 0))
        {
            var producto = await _context.Productos.FindAsync(item.ProductoId);
            if (producto == null || !producto.Activo || producto.PrecioActual == null) continue;

            var precioUnitario = producto.PrecioActual.Value;
            var subtotal = precioUnitario * item.Cantidad;

            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = subtotal
            });

            total += subtotal;
        }

        if (!venta.Detalles.Any())
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Ninguno de los productos seleccionados es válido." };
        }

        venta.Total = total;

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }
}