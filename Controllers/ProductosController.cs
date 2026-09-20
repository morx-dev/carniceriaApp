using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Controllers;

[Authorize(Roles = "Administrador")]
public class ProductosController : Controller
{
    private readonly IProductoService _productoService;
    private readonly UserManager<Usuario> _userManager;

    public ProductosController(IProductoService productoService, UserManager<Usuario> userManager)
    {
        _productoService = productoService;
        _userManager = userManager;
    }

    // GET: Productos
    public async Task<IActionResult> Index(string? busqueda, CategoriaProducto? categoria)
    {
        var productos = await _productoService.ObtenerProductosAsync(busqueda, categoria);

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
        if (!ModelState.IsValid)
            return View(producto);

        var resultado = await _productoService.CrearAsync(producto);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError!);
            return View(producto);
        }

        TempData["Mensaje"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: Productos/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Producto productoEditado)
    {
        if (id != productoEditado.Id) return NotFound();
        if (!ModelState.IsValid) return View(productoEditado);

        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        var resultado = await _productoService.EditarAsync(id, productoEditado, usuarioId);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError!);
            return View(productoEditado);
        }

        TempData["Mensaje"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Productos/Eliminar/5
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: Productos/Eliminar/5
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        await _productoService.DesactivarAsync(id);
        TempData["Mensaje"] = "Producto desactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}