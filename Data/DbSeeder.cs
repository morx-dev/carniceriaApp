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
        var config = serviceProvider.GetRequiredService<IConfiguration>();

        string[] roles = { "Administrador", "CallCenter", "Mostrador", "Repartidor" };

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        // El admin inicial se lee de la configuración (.env / appsettings)
        string? adminEmail = config["AdminInicial:Email"];
        string? adminPassword = config["AdminInicial:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return; // Sin datos configurados no se crea ningún administrador
        }

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
}