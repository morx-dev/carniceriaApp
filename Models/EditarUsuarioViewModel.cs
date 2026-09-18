using System.ComponentModel.DataAnnotations;

namespace carniceriaApp.Models;

public class EditarUsuarioViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre completo no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un rol")]
    public string Rol { get; set; } = string.Empty;
}