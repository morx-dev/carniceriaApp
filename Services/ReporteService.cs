using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp.Services.Interfaces;

namespace carniceriaApp.Services;

public class ReporteService : IReporteService
{
    private readonly ApplicationDbContext _context;

    private static readonly DateTime SinCorte = new DateTime(9000, 1, 1);

    public ReporteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CuadreDiario?> ObtenerCuadrePorFechaAsync(DateTime fecha, TipoNegocio negocio)
    {
        if (!Enum.IsDefined(typeof(TipoNegocio), negocio)) return null;

        var fechaSolo = fecha.Date;
        return await _context.CuadresDiarios
            .FirstOrDefaultAsync(c => c.Fecha == fechaSolo && c.Negocio == negocio);
    }

    private async Task<(DateTime CorteDia, DateTime CorteAnterior)> ObtenerCortesAsync(DateTime fechaSolo, TipoNegocio negocio)
    {
        var anterior = fechaSolo.AddDays(-1);

        var cuadres = await _context.CuadresDiarios
            .Where(c => c.Negocio == negocio && (c.Fecha == fechaSolo || c.Fecha == anterior))
            .ToListAsync();

        var corteDia = cuadres.FirstOrDefault(c => c.Fecha == fechaSolo)?.FechaCreacion ?? SinCorte;
        var corteAnterior = cuadres.FirstOrDefault(c => c.Fecha == anterior)?.FechaCreacion ?? SinCorte;

        return (corteDia, corteAnterior);
    }

    private IQueryable<Venta> VentasPresencialesDelDia(DateTime dia, TipoNegocio negocio, DateTime corteDia, DateTime corteAnterior)
    {
        var anterior = dia.AddDays(-1);

        return _context.Ventas.Where(v =>
            v.TipoOrigen == TipoOrigen.Presencial &&
            v.Negocio == negocio &&
            (
                (v.FechaCreacion.Date == dia && v.FechaCreacion <= corteDia) ||
                (v.FechaCreacion.Date == anterior && v.FechaCreacion > corteAnterior)
            ));
    }

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

    private static bool EsLineaAntojitos(DetalleVenta d) =>
        d.EsAntojito;

    private static bool LineaPerteneceANegocio(DetalleVenta d, TipoNegocio negocio) =>
        negocio == TipoNegocio.Antojitos ? EsLineaAntojitos(d) : !EsLineaAntojitos(d);

    private async Task<(List<Venta> Presenciales, List<Venta> Remotas)> CargarVentasDelNegocioAsync(DateTime fechaSolo, TipoNegocio negocio)
    {
        var (corteDia, corteAnterior) = await ObtenerCortesAsync(fechaSolo, negocio);

        var presenciales = await VentasPresencialesDelDia(fechaSolo, negocio, corteDia, corteAnterior)
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .ToListAsync();

        var remotasCandidatas = await VentasRemotasEntregadasDelDia(fechaSolo, corteDia, corteAnterior)
            .Include(v => v.Cliente)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .ToListAsync();

        var remotas = remotasCandidatas
            .Where(v => v.Detalles != null && v.Detalles.Any(d => LineaPerteneceANegocio(d, negocio)))
            .ToList();

        return (presenciales, remotas);
    }

    public async Task<ResumenNegocio> CalcularResumenDelDiaAsync(DateTime fecha, TipoNegocio negocio)
    {
        if (!Enum.IsDefined(typeof(TipoNegocio), negocio))
        {
            return new ResumenNegocio { Presencial = 0, Remoto = 0 };
        }

        var fechaSolo = fecha.Date;

        var cuadreExistente = await ObtenerCuadrePorFechaAsync(fechaSolo, negocio);
        if (cuadreExistente != null)
        {
            return new ResumenNegocio
            {
                Presencial = cuadreExistente.TotalVentasPresenciales,
                Remoto = cuadreExistente.TotalVentasSistema
            };
        }

        var (presenciales, remotas) = await CargarVentasDelNegocioAsync(fechaSolo, negocio);

        return new ResumenNegocio
        {
            Presencial = presenciales.Sum(v => v.Total),
            Remoto = remotas.Sum(v => v.Detalles
                .Where(d => LineaPerteneceANegocio(d, negocio))
                .Sum(d => d.Subtotal))
        };
    }

    public async Task<List<CuadreDiario>> ObtenerHistorialAsync()
    {
        return await _context.CuadresDiarios
            .OrderByDescending(c => c.Fecha)
            .ThenBy(c => c.Negocio)
            .ToListAsync();
    }

    public async Task<List<Venta>> ObtenerVentasDelDiaAsync(DateTime fecha)
    {
        var fechaSolo = fecha.Date;

        var (presCarniceria, remCarniceria) = await CargarVentasDelNegocioAsync(fechaSolo, TipoNegocio.Carniceria);
        var (presAntojitos, remAntojitos) = await CargarVentasDelNegocioAsync(fechaSolo, TipoNegocio.Antojitos);

        var presenciales = presCarniceria.Concat(presAntojitos).GroupBy(v => v.Id).Select(g => g.First());
        var remotas = remCarniceria.Concat(remAntojitos).GroupBy(v => v.Id).Select(g => g.First());

        return presenciales.Concat(remotas).ToList();
    }

    public async Task<List<DateTime>> ObtenerDiasPendientesDeCierreAsync(TipoNegocio negocio)
    {
        if (!Enum.IsDefined(typeof(TipoNegocio), negocio)) return new List<DateTime>();

        var ayer = DateTime.Today.AddDays(-1);
        var desde = DateTime.Today.AddDays(-60);
        var esAntojitos = negocio == TipoNegocio.Antojitos;

        var fechasConVentasPresenciales = await _context.Ventas
            .Where(v => v.TipoOrigen == TipoOrigen.Presencial
                     && v.Negocio == negocio
                     && v.FechaCreacion.Date <= ayer
                     && v.FechaCreacion.Date >= desde)
            .Select(v => v.FechaCreacion.Date)
            .Distinct()
            .ToListAsync();

        // Solo cuentan las remotas que tienen al menos una línea de ESTE negocio
        var fechasConVentasRemotas = await _context.Ventas
            .Where(v => v.TipoOrigen != TipoOrigen.Presencial
                     && v.EstadoId == 4
                     && v.FechaEntrega.HasValue
                     && v.FechaEntrega.Value.Date <= ayer
                     && v.FechaEntrega.Value.Date >= desde
                     && v.Detalles.Any(d => d.EsAntojito == esAntojitos))
            .Select(v => v.FechaEntrega!.Value.Date)
            .Distinct()
            .ToListAsync();

        var todasLasFechas = fechasConVentasPresenciales.Union(fechasConVentasRemotas).OrderBy(f => f).ToList();

        var fechasCerradas = await _context.CuadresDiarios
            .Where(c => c.Negocio == negocio && c.Fecha <= ayer)
            .Select(c => c.Fecha)
            .ToListAsync();

        return todasLasFechas.Except(fechasCerradas).ToList();
    }

    public async Task<ResultadoOperacion> CerrarDiaAsync(string usuarioId, TipoNegocio negocio, DateTime? fecha = null)
    {
        if (!Enum.IsDefined(typeof(TipoNegocio), negocio))
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "El tipo de negocio proporcionado no es válido." };
        }

        var dia = (fecha ?? DateTime.Today).Date;
        var nombreNegocio = negocio == TipoNegocio.Antojitos ? "Antojitos" : "Carnicería";

        if (dia > DateTime.Today)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = "No se puede cerrar un día futuro." };
        }

        var esDiaPasado = dia < DateTime.Today;

        var yaExiste = await _context.CuadresDiarios.AnyAsync(c => c.Fecha == dia && c.Negocio == negocio);
        if (yaExiste)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = $"Ese día ya fue cerrado para {nombreNegocio}." };
        }

        var resumen = await CalcularResumenDelDiaAsync(dia, negocio);

        var cuadre = new CuadreDiario
        {
            Fecha = dia,
            UsuarioId = usuarioId,
            Negocio = negocio,
            TotalVentasPresenciales = resumen.Presencial,
            TotalVentasSistema = resumen.Remoto,
            TotalGeneral = resumen.Total,
            // Día pasado: 23:59:59.999999 (MySQL guarda hasta microsegundos, por eso -10 ticks)
            FechaCreacion = esDiaPasado ? dia.AddDays(1).AddTicks(-10) : DateTime.Now
        };

        _context.CuadresDiarios.Add(cuadre);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = $"Ese día ya fue cerrado para {nombreNegocio}." };
        }

        return new ResultadoOperacion { Exitoso = true };
    }
}