using System.ComponentModel.DataAnnotations;

namespace carniceriaApp.Models;

public enum CategoriaProducto
{
    Res,
    Cerdo,
    Pollo,
    Embutidos,
    Menudos
}

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public CategoriaProducto Categoria { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 1000000.00, ErrorMessage = "El precio actual debe ser mayor a cero.")]
    public decimal? PrecioActual { get; set; }

    public bool Activo { get; set; } = true;
}