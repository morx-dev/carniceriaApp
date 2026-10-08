using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _context;

    // Valor "nunca": se usa cuando un día todavía no tiene cuadre cerrado (no hay hora de corte)
    private static readonly DateTime SinCorte = new DateTime(9000, 1, 1);

    public ReporteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CuadreDiario?> ObtenerCuadrePorFechaAsync(DateTime fecha)
    {
        var fechaSolo = fecha.Date;
        return await _context.CuadresDiarios.FirstOrDefaultAsync(c => c.Fecha == fechaSolo);
    }

    // Hora de corte del día pedido y del día anterior (si ya fueron cerrados)
    private async Task<(DateTime CorteDia, DateTime CorteAnterior)> ObtenerCortesAsync(DateTime fechaSolo)
    {
        var anterior = fechaSolo.AddDays(-1);

        var cuadres = await _context.CuadresDiarios
            .Where(c => c.Fecha == fechaSolo || c.Fecha == anterior)
            .ToListAsync();

        var corteDia = cuadres.FirstOrDefault(c => c.Fecha == fechaSolo)?.FechaCreacion ?? SinCorte;
        var corteAnterior = cuadres.FirstOrDefault(c => c.Fecha == anterior)?.FechaCreacion ?? SinCorte;

        return (corteDia, corteAnterior);
    }

    // Mostrador del día: lo creado ese día ANTES del cierre + lo creado el día anterior DESPUÉS de su cierre
    private IQueryable<Venta> VentasPresencialesDelDia(DateTime dia, DateTime corteDia, DateTime corteAnterior)
    {
        var anterior = dia.AddDays(-1);

        return _context.Ventas.Where(v =>
            v.TipoOrigen == TipoOrigen.Presencial &&
            (
                (v.FechaCreacion.Date == dia && v.FechaCreacion <= corteDia) ||
                (v.FechaCreacion.Date == anterior && v.FechaCreacion > corteAnterior)
            ));
    }

    // Remotas del día: lo entregado ese día ANTES del cierre + lo entregado el día anterior DESPUÉS de su cierre
    private IQueryable<Venta> VentasRemotasEntregadasDelDia(DateTime dia, DateTime corteDia, DateTime corteAnterior)
    {
        var anterior = dia.AddDays(-1);

        return _context.Ventas.Where(v =>
            v.TipoOrigen != TipoOrigen.Presencial &&
            v.EstadoId == 4 &&
            v.FechaEntrega.HasValue &&
            (
                (v.FechaEntrega.Value.Date == dia && v.FechaEntrega.Value <= corteDia) ||
                (v.FechaEntrega.Value.Date == anterior && v.FechaEntrega.Value > corteAnterior)
            ));
    }

    public async Task<ResumenCuadre> CalcularResumenDelDiaAsync(DateTime fecha)
    {
        var fechaSolo = fecha.Date;
        var (corteDia, corteAnterior) = await ObtenerCortesAsync(fechaSolo);

        var totalPresencial = await VentasPresencialesDelDia(fechaSolo, corteDia, corteAnterior)
            .SumAsync(v => (decimal?)v.Total) ?? 0;

        var totalSistema = await VentasRemotasEntregadasDelDia(fechaSolo, corteDia, corteAnterior)
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

    public async Task<List<Venta>> ObtenerVentasDelDiaAsync(DateTime fecha)
    {
        var fechaSolo = fecha.Date;
        var (corteDia, corteAnterior) = await ObtenerCortesAsync(fechaSolo);

        var presenciales = await VentasPresencialesDelDia(fechaSolo, corteDia, corteAnterior)
            .Include(v => v.Cliente)
            .ToListAsync();

        var remotasEntregadas = await VentasRemotasEntregadasDelDia(fechaSolo, corteDia, corteAnterior)
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .ToListAsync();

        return presenciales.Concat(remotasEntregadas).ToList();
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