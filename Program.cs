using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using carniceriaApp.Data;
using carniceriaApp.Models;
using carniceriaApp; // <-- Asegura que reconozca el factory creado en la raíz

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión a MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext con MySQL (Pomelo)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Identity con roles (sin Razor Pages, para mantenerlo 100% MVC)
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// REGISTRO PARA QUE EL NAVBAR MUESTRE EL NOMBRE COMPLETO AUTOMÁTICAMENTE
builder.Services.AddScoped<IUserClaimsPrincipalFactory<Usuario>, AdditionalUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // IMPORTANTE: siempre antes de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

// Seed de roles y usuario Administrador inicial
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    await DbSeeder.SeedRolesYAdminAsync(services);
    await DbSeeder.SeedProductosAsync(context);
}

app.Run();