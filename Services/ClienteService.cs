using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> ObtenerClientesAsync(string? busqueda, int? sector)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(c => c.Nombre.Contains(busqueda) || c.Telefono.Contains(busqueda));
        }

        if (sector.HasValue)
        {
            query = query.Where(c => (int)c.Sector == sector.Value);
        }

        return await query.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task<ResultadoOperacion> CrearAsync(Cliente cliente)
    {
        cliente.Nombre = cliente.Nombre?.Trim() ?? string.Empty;
        cliente.Telefono = cliente.Telefono?.Trim() ?? string.Empty;
        cliente.Direccion = cliente.Direccion?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cliente.Nombre))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El nombre es obligatorio.", Campo = "Nombre" };
        }

        if (string.IsNullOrWhiteSpace(cliente.Telefono))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El teléfono es obligatorio.", Campo = "Telefono" };
        }

        bool existeDuplicado = await _context.Clientes.AnyAsync(c =>
            c.Activo && c.Telefono == cliente.Telefono);

        if (existeDuplicado)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                MensajeError = "Ya existe un cliente activo registrado con ese número de teléfono.",
                Campo = "Telefono"
            };
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task<ResultadoOperacion> EditarAsync(int id, Cliente clienteEditado)
    {
        var clienteActual = await _context.Clientes.FindAsync(id);
        if (clienteActual == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Cliente no encontrado." };
        }

        var nombreLimpio = clienteEditado.Nombre?.Trim() ?? string.Empty;
        var telefonoLimpio = clienteEditado.Telefono?.Trim() ?? string.Empty;
        var direccionLimpia = clienteEditado.Direccion?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El nombre es obligatorio.", Campo = "Nombre" };
        }

        if (string.IsNullOrWhiteSpace(telefonoLimpio))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El teléfono es obligatorio.", Campo = "Telefono" };
        }

        bool existeDuplicado = await _context.Clientes.AnyAsync(c =>
            c.Id != id && c.Activo && c.Telefono == telefonoLimpio);

        if (existeDuplicado)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                MensajeError = "Ya existe otro cliente activo registrado con ese número de teléfono.",
                Campo = "Telefono"
            };
        }

        clienteActual.Nombre = nombreLimpio;
        clienteActual.Telefono = telefonoLimpio;
        clienteActual.Sector = clienteEditado.Sector;
        clienteActual.Direccion = direccionLimpia;

        await _context.SaveChangesAsync();

        return new ResultadoOperacion { Exitoso = true };
    }

    public async Task CambiarEstadoAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return;

        cliente.Activo = !cliente.Activo;
        await _context.SaveChangesAsync();
    }
}