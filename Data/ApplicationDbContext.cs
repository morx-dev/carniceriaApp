using carniceriaApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace carniceriaApp.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Producto> Productos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<EstadoVenta> EstadosVenta { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<DetalleVenta> DetalleVentas { get; set; }
    public DbSet<HistorialPrecio> HistorialPrecios { get; set; }
    public DbSet<CuadreDiario> CuadresDiarios { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Venta>()
            .HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Venta>()
            .HasOne(v => v.Estado)
            .WithMany()
            .HasForeignKey(v => v.EstadoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EstadoVenta>().HasData(
            new EstadoVenta { Id = 1, Nombre = "Pendiente" },
            new EstadoVenta { Id = 2, Nombre = "En preparacion" },
            new EstadoVenta { Id = 3, Nombre = "En camino" },
            new EstadoVenta { Id = 4, Nombre = "Entregado" },
            new EstadoVenta { Id = 5, Nombre = "Cancelado" }
        );
    }
}