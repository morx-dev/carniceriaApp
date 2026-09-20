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

        // Filtro por nombre o teléfono
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(c => c.Nombre.Contains(busqueda) || c.Telefono.Contains(busqueda));
        }

        // Filtro por sector residencial
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

    public async Task CrearAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task<ResultadoOperacion> EditarAsync(int id, Cliente clienteEditado)
    {
        var clienteActual = await _context.Clientes.FindAsync(id);
        if (clienteActual == null)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "Cliente no encontrado." };
        }

        clienteActual.Nombre = clienteEditado.Nombre;
        clienteActual.Telefono = clienteEditado.Telefono;
        clienteActual.Sector = clienteEditado.Sector;
        clienteActual.Direccion = clienteEditado.Direccion;

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