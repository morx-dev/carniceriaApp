using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using carniceriaApp.Models;

namespace carniceriaApp.Data;

public class DbSeeder
{
    public static async Task SeedRolesYAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

        string[] roles = { "Administrador", "CallCenter", "Mostrador", "Repartidor" };

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        string adminEmail = "gustavoestrada@gmail.com";
        string adminPassword = "W@4nB%hc";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                NombreCompleto = "Administrador General",
                Activo = true
            };

            var resultado = await userManager.CreateAsync(adminUser, adminPassword);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }
    }

    // NUEVO MÉTODO PARA POBLAR LOS PRODUCTOS
    public static async Task SeedProductosAsync(ApplicationDbContext context)
    {
        if (context.Productos.Any())
        {
            return; // Si ya hay productos, no hace nada
        }

        var productosIniciales = new List<Producto>
        {
            new Producto { Nombre = "Lomito de Res", Categoria = CategoriaProducto.Res, PrecioActual = 75.00m, Activo = true },
            new Producto { Nombre = "Churrasco", Categoria = CategoriaProducto.Res, PrecioActual = 48.50m, Activo = true },
            new Producto { Nombre = "Costilla de Res", Categoria = CategoriaProducto.Res, PrecioActual = 32.00m, Activo = true },
            new Producto { Nombre = "Carne Molida Especial", Categoria = CategoriaProducto.Res, PrecioActual = 35.00m, Activo = true },
            
            new Producto { Nombre = "Costilla de Cerdo", Categoria = CategoriaProducto.Cerdo, PrecioActual = 34.00m, Activo = true },
            new Producto { Nombre = "Chuleta Ahumada", Categoria = CategoriaProducto.Cerdo, PrecioActual = 38.00m, Activo = true },
            new Producto { Nombre = "Posta de Cerdo", Categoria = CategoriaProducto.Cerdo, PrecioActual = 32.00m, Activo = true },

            new Producto { Nombre = "Pechuga de Pollo Entera", Categoria = CategoriaProducto.Pollo, PrecioActual = 28.00m, Activo = true },
            new Producto { Nombre = "Pierna y Muslo", Categoria = CategoriaProducto.Pollo, PrecioActual = 22.00m, Activo = true },
            new Producto { Nombre = "Alitas Adobadas", Categoria = CategoriaProducto.Pollo, PrecioActual = 30.00m, Activo = true },

            new Producto { Nombre = "Chorizo Cobanero", Categoria = CategoriaProducto.Embutidos, PrecioActual = 25.00m, Activo = true },
            new Producto { Nombre = "Longaniza de Res", Categoria = CategoriaProducto.Embutidos, PrecioActual = 26.00m, Activo = true },

            new Producto { Nombre = "Hígado de Res", Categoria = CategoriaProducto.Menudos, PrecioActual = 18.00m, Activo = true },
            new Producto { Nombre = "Panza de Res", Categoria = CategoriaProducto.Menudos, PrecioActual = 15.00m, Activo = true }
        };

        context.Productos.AddRange(productosIniciales);
        await context.SaveChangesAsync();
    }
}