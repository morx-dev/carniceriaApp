using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public VentaService(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<Producto>> ObtenerProductosActivosAsync()
    {
        return await _context.Productos.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
    }

    public async Task<List<Cliente>> ObtenerClientesActivosAsync()
    {
        return await _context.Clientes.Where(c => c.Activo).OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<List<Usuario>> ObtenerRepartidoresActivosAsync()
    {
        var repartidores = await _userManager.GetUsersInRoleAsync("Repartidor");
        return repartidores.Where(u => u.Activo).OrderBy(u => u.NombreCompleto).ToList();
    }

    public async Task<List<Venta>> ObtenerPendientesAsync()
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Where(v => v.EstadoId == 1) // Pendiente
            .OrderBy(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<Venta?> ObtenerVentaConDetalleAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<ResultadoOperacion> CrearVentaPresencialAsync(CrearVentaViewModel modelo, string usuarioId)
    {
        if (modelo.MontoTotalPresencial == null || modelo.MontoTotalPresencial <= 0)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Ingresa un monto válido." };
        }

        var venta = new Venta
        {
            ClienteId = null,
            TipoOrigen = TipoOrigen.Presencial,
            EstadoId = 4, // Entregado — inmediato
            UsuarioCreadorId = usuarioId,
            Total = modelo.MontoTotalPresencial.Value,
            FechaCreacion = DateTime.Now,
            FechaEntrega = DateTime.Now
        };

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task<ResultadoOperacion> CrearVentaRemotaAsync(CrearVentaViewModel modelo, string usuarioId)
    {
        if (modelo.ClienteId == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Debes seleccionar un cliente." };
        }

        bool usaObservacion = !string.IsNullOrWhiteSpace(modelo.Observaciones);

        var venta = new Venta
        {
            ClienteId = modelo.ClienteId,
            TipoOrigen = modelo.TipoOrigenSeleccionado ?? TipoOrigen.WhatsApp,
            EstadoId = 1, // Pendiente
            UsuarioCreadorId = usuarioId,
            FechaCreacion = DateTime.Now
        };

        if (usaObservacion)
        {
            venta.Observaciones = modelo.Observaciones;
            venta.Total = 0;

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return new ResultadoOperacion { Exitoso = true };
        }

        return await ArmarYGuardarVentaAsync(venta, modelo.Detalles);
    }

    public async Task<ResultadoOperacion> ActualizarDetallesAsync(int ventaId, List<DetalleVentaInputViewModel> detalles, string? observaciones)
    {
        var venta = await _context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Venta no encontrada." };
        }

        if (venta.EstadoId != 1)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Esta venta ya no se puede editar." };
        }

        _context.DetalleVentas.RemoveRange(venta.Detalles);
        venta.Detalles.Clear();

        decimal total = 0;

        if (detalles != null)
        {
            foreach (var item in detalles.Where(d => d.Cantidad > 0))
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null || producto.PrecioActual == null) continue;

                var subtotal = producto.PrecioActual.Value * item.Cantidad;

                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.PrecioActual.Value,
                    Subtotal = subtotal
                });

                total += subtotal;
            }
        }

        venta.Total = total;

        if (observaciones != null)
        {
            venta.Observaciones = observaciones;
        }

        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task<ResultadoOperacion> AsignarRepartidorAsync(int ventaId, string repartidorId)
    {
        var venta = await _context.Ventas.FindAsync(ventaId);
        if (venta == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Venta no encontrada." };
        }

        if (venta.EstadoId != 1)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Esta venta ya fue asignada o no está pendiente." };
        }

        venta.RepartidorId = repartidorId;
        venta.EstadoId = 3; // En camino

        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
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

    public async Task<List<Venta>> ObtenerAsignadasARepartidorAsync(string repartidorId)
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Where(v => v.RepartidorId == repartidorId && v.EstadoId == 3) // En camino
            .OrderBy(v => v.FechaCreacion)
            .ToListAsync();
    }

    public async Task<ResultadoOperacion> MarcarEntregadoAsync(int ventaId, string repartidorId)
    {
        var venta = await _context.Ventas.FindAsync(ventaId);
        if (venta == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Venta no encontrada." };
        }

        if (venta.RepartidorId != repartidorId)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Este pedido no está asignado a ti." };
        }

        if (venta.EstadoId != 3)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Este pedido no está en camino." };
        }

        venta.EstadoId = 4; // Entregado
        venta.FechaEntrega = DateTime.Now;

        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }


    public async Task<List<Venta>> ObtenerTodasEnCaminoAsync()
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Where(v => v.EstadoId == 3) // En camino
            .OrderBy(v => v.FechaCreacion)
            .ToListAsync();
    }


}