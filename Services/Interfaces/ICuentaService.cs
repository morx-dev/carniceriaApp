using carniceriaApp.Models;

namespace carniceriaApp.Services.Interfaces;

public class ResultadoLogin
{
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }
    public string? ControllerDestino { get; set; }
    public string? AccionDestino { get; set; }
}

public interface ICuentaService
{
    Task<ResultadoLogin> IniciarSesionAsync(string email, string password, bool recordarMe);
    Task CerrarSesionAsync();
}