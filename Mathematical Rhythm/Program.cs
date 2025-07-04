using Microsoft.EntityFrameworkCore;
using MathematicalRhythm.Data;
using MathematicalRhythm.Models;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregar EF Core con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

//Tronar Sesion
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "MathematicalRhythm.Session";
    options.Cookie.HttpOnly = true; // Protege contra XSS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Protege contra CSRF
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración
});

//EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

//Recaptcha
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<GoogleRecaptchaConfig>(builder.Configuration.GetSection("GoogleRecaptcha"));
builder.Services.AddHttpClient<RecaptchaService>(client =>
{
    client.BaseAddress = new Uri("https://www.google.com/recaptcha/api/siteverify");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Agrega servicios al contenedor.
builder.Services.AddControllersWithViews();
builder.Services.AddCors();  // Habilita CORS

var app = builder.Build();

// Configura el pipeline de HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}





// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




//Tronar sesion
app.Use(async (context, next) =>
{
    context.Response.GetTypedHeaders().CacheControl =
        new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MustRevalidate = true
        };
    // Aplicar a todas las respuestas HTTP
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "-1";

    await next();
});




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// Habilita CORS - Colócalo después de UseRouting y antes de UseAuthorization
app.UseCors(policy => policy
    .WithOrigins("https://localhost:7199") // Cambia al puerto de tu frontend
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSession();
app.Run();
