using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class ProductosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public ProductosController(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Productos
    public async Task<IActionResult> Index(string? busqueda, CategoriaProducto? categoria)
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

        var productos = await query
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        ViewBag.BusquedaActual = busqueda;
        ViewBag.CategoriaActual = categoria;

        // Si es una petición AJAX, devolvemos solo una vista parcial o un fragmento con la tabla
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_TablaProductos", productos);
        }

        return View(productos);
    }

    // GET: Productos/Crear
    public IActionResult Crear()
    {
        return View();
    }

    // POST: Productos/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Producto producto)
    {
        // Validar si ya existe un producto idéntico activo
        bool existeDuplicado = await _context.Productos.AnyAsync(p => 
            p.Activo &&
            p.Nombre.ToLower() == producto.Nombre.ToLower() && 
            p.Categoria == producto.Categoria && 
            p.PrecioActual == producto.PrecioActual);

        if (existeDuplicado)
        {
            ModelState.AddModelError(string.Empty, "Ya existe un producto activo registrado con el mismo nombre, categoría y precio.");
        }

        if (!ModelState.IsValid)
            return View(producto);

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: Productos/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Producto productoEditado)
    {
        if (id != productoEditado.Id) return NotFound();

        // Validar si otro producto diferente ya tiene los mismos datos exactos
        bool existeDuplicado = await _context.Productos.AnyAsync(p => 
            p.Id != id &&
            p.Activo &&
            p.Nombre.ToLower() == productoEditado.Nombre.ToLower() && 
            p.Categoria == productoEditado.Categoria && 
            p.PrecioActual == productoEditado.PrecioActual);

        if (existeDuplicado)
        {
            ModelState.AddModelError(string.Empty, "Ya existe otro producto activo con el mismo nombre, categoría y precio.");
        }

        if (!ModelState.IsValid) return View(productoEditado);

        var productoActual = await _context.Productos.FindAsync(id);
        if (productoActual == null) return NotFound();

        // Si cambió el precio, registrar en el historial
        if (productoActual.PrecioActual != productoEditado.PrecioActual)
        {
            var usuarioId = _userManager.GetUserId(User) ?? string.Empty;

            _context.HistorialPrecios.Add(new HistorialPrecio
            {
                ProductoId = productoActual.Id,
                PrecioAnterior = productoActual.PrecioActual,
                PrecioNuevo = productoEditado.PrecioActual,
                UsuarioId = usuarioId
            });
        }

        productoActual.Nombre = productoEditado.Nombre;
        productoActual.Categoria = productoEditado.Categoria;
        productoActual.PrecioActual = productoEditado.PrecioActual;
        productoActual.Activo = productoEditado.Activo;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Eliminar/5
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: Productos/Eliminar/5
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            producto.Activo = false; // Baja lógica, no borrado físico (por el historial de precios ligado)
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}