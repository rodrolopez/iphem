// --- INICIO DE CONFIGURACIÓN DE BASE DE DATOS ---
// Agregamos los "usings" necesarios para que entienda el código
using Microsoft.EntityFrameworkCore;
using iphem_web.Models; // Asegurate que coincida con el nombre de tu proyecto
using Microsoft.AspNetCore.Authentication.Cookies;

// Esta línea desactiva la restricción estricta de fechas de PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// CONFIGURACIÓN DE LOGIN (COOKIES)
// <--- INICIO BLOQUE NUEVO --->
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        // Si alguien quiere entrar sin permiso, lo mandamos acá:
        option.LoginPath = "/Acceso/Login"; 
        // La cookie expira en 20 minutos de inactividad
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20); 
    });
// <--- FIN BLOQUE NUEVO --->

// Definimos la cadena de conexión (hardcoded para desarrollo)
var connectionString = "Host=localhost;Database=iphem_db;Username=postgres;Password=postgres";

// Inyectamos el contexto de la base de datos al sistema
builder.Services.AddDbContext<IphemDbContext>(options =>
    options.UseNpgsql(connectionString));
// --- FIN DE CONFIGURACIÓN ---
// Add services to the container.
builder.Services.AddControllersWithViews();

// Esto le permite a cualquier parte del sistema saber quién está logueado
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// <--- OJO ACÁ: El orden importa. Authentication va ANTES de Authorization
app.UseAuthentication(); // <--- NUEVO (El portero verifica quién sos)
app.UseAuthorization();  // <--- (El seguridad verifica a dónde podés entrar)

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();