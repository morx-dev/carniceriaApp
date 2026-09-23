using Microsoft.AspNetCore.Identity;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class CuentaService : ICuentaService
{
    private readonly SignInManager<Usuario> _signInManager;
    private readonly UserManager<Usuario> _userManager;

    public CuentaService(SignInManager<Usuario> signInManager, UserManager<Usuario> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<ResultadoLogin> IniciarSesionAsync(string email, string password, bool recordarMe)
    {
        var usuario = await _userManager.FindByEmailAsync(email);

        if (usuario != null && !usuario.Activo)
        {
            return new ResultadoLogin
            {
                Exitoso = false,
                MensajeError = "Este usuario está desactivado. Contacta al administrador."
            };
        }

        var resultado = await _signInManager.PasswordSignInAsync(email, password, recordarMe, lockoutOnFailure: false);

        if (!resultado.Succeeded)
        {
            return new ResultadoLogin
            {
                Exitoso = false,
                MensajeError = "Correo o contraseña incorrectos."
            };
        }

        var (controller, accion) = await ObtenerDestinoSegunRolAsync(usuario!);

        return new ResultadoLogin
        {
            Exitoso = true,
            ControllerDestino = controller,
            AccionDestino = accion
        };
    }

    private async Task<(string controller, string accion)> ObtenerDestinoSegunRolAsync(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);

        if (roles.Contains("Administrador"))
            return ("Productos", "Index");

        if (roles.Contains("CallCenter"))
            return ("Ventas", "Crear");

        if (roles.Contains("Mostrador"))
            return ("Ventas", "Crear");

        // TODO: cuando exista la acción MisEntregas en VentasController, cambiar a ("Ventas", "MisEntregas")
        if (roles.Contains("Repartidor"))
            return ("Home", "Index");

        return ("Home", "Index");
    }

    public async Task CerrarSesionAsync()
    {
        await _signInManager.SignOutAsync();
    }
}