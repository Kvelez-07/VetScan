using AspNetCore.ReCaptcha;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Data.Services;
using VetScan.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuración de sesión (AGREGAR ESTO)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Tiempo de expiración
    options.Cookie.HttpOnly = true; // Seguridad básica
    options.Cookie.IsEssential = true; // Obligatorio para GDPR
});

// Email services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Google reCAPTCHA configuration
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));

// DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Agregar el middleware de sesión (AGREGAR ESTO)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
