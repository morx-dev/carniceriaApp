using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador,CallCenter,Mostrador")]
public class ClientesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? busqueda)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(c => c.Nombre.Contains(busqueda) || c.Telefono.Contains(busqueda));
        }

        var clientes = await query.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.BusquedaActual = busqueda;

        // Soporte AJAX igual que Productos y Usuarios
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TablaClientes", clientes);
        }

        return View(clientes);
    }

    public IActionResult Crear()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cliente cliente)
    {
        if (!ModelState.IsValid)
            return View(cliente);

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();
        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Cliente clienteEditado)
    {
        if (id != clienteEditado.Id) return NotFound();
        if (!ModelState.IsValid) return View(clienteEditado);

        var clienteActual = await _context.Clientes.FindAsync(id);
        if (clienteActual == null) return NotFound();

        clienteActual.Nombre = clienteEditado.Nombre;
        clienteActual.Telefono = clienteEditado.Telefono;
        clienteActual.Direccion = clienteEditado.Direccion;

        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        cliente.Activo = !cliente.Activo;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}