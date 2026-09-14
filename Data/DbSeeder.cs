using Microsoft.AspNetCore.Identity;

namespace carniceriaApp.Data;

public static class DbSeeder
{
    public static async Task SeedRolesYAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roles = { "Administrador", "CallCenter", "Mostrador", "Repartidor" };

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        string adminEmail = "gustavoestrada@gmail.com";
        string adminPassword = "W@4nB%hC";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var resultado = await userManager.CreateAsync(adminUser, adminPassword);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }
    }
}