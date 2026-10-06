using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _context;

    public ReporteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CuadreDiario?> ObtenerCuadrePorFechaAsync(DateTime fecha)
    {
        var fechaSolo = fecha.Date;
        return await _context.CuadresDiarios.FirstOrDefaultAsync(c => c.Fecha == fechaSolo);
    }

    public async Task<ResumenCuadre> CalcularResumenDelDiaAsync(DateTime fecha)
    {
        var fechaSolo = fecha.Date;

        var totalPresencial = await _context.Ventas
            .Where(v => v.TipoOrigen == TipoOrigen.Presencial && v.FechaCreacion.Date == fechaSolo)
            .SumAsync(v => (decimal?)v.Total) ?? 0;

        var totalSistema = await _context.Ventas
            .Where(v => v.TipoOrigen != TipoOrigen.Presencial
                     && v.EstadoId == 4
                     && v.FechaEntrega.HasValue
                     && v.FechaEntrega.Value.Date == fechaSolo)
            .SumAsync(v => (decimal?)v.Total) ?? 0;

        return new ResumenCuadre
        {
            TotalPresencial = totalPresencial,
            TotalSistema = totalSistema,
            TotalGeneral = totalPresencial + totalSistema
        };
    }

    public async Task<List<CuadreDiario>> ObtenerHistorialAsync()
    {
        return await _context.CuadresDiarios
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
    }

    public async Task<ResultadoOperacion> CerrarDiaAsync(string usuarioId)
    {
        var hoy = DateTime.Today;

        var yaExiste = await _context.CuadresDiarios.AnyAsync(c => c.Fecha == hoy);
        if (yaExiste)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El día de hoy ya fue cerrado." };
        }

        var resumen = await CalcularResumenDelDiaAsync(hoy);

        var cuadre = new CuadreDiario
        {
            Fecha = hoy,
            UsuarioId = usuarioId,
            TotalVentasPresenciales = resumen.TotalPresencial,
            TotalVentasSistema = resumen.TotalSistema,
            TotalGeneral = resumen.TotalGeneral,
            FechaCreacion = DateTime.Now
        };

        _context.CuadresDiarios.Add(cuadre);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Protección por si dos clics simultáneos intentan cerrar el mismo día
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El día de hoy ya fue cerrado." };
        }

        return new ResultadoOperacion { Exitoso = true };
    }
}