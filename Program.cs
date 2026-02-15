using System.Text.Json;
using BookingSystemAPI.Repositories;
using BookingSystemAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------ PORT CONFIG FOR RENDER ------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ------------------ SERVICES ------------------

// Controllers
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Razor Pages
builder.Services.AddRazorPages();

// Dependency Injection
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

// ------------------ MIDDLEWARE ------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// IMPORTANT: On Render we DO NOT need HTTPS redirection
// Because Render already provides HTTPS
// So disable it to avoid port issues

// app.UseHttpsRedirection();  <-- COMMENTED OUT

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

