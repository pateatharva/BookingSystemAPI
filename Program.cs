using System.Text.Json;
using BookingSystemAPI.Repositories;
using BookingSystemAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//
// ================= PORT CONFIG (IMPORTANT FOR RENDER) =================
//

// If running on Render → use PORT
// If running locally → fallback to 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

//
// ================= SERVICES =================
//

// Controllers (API)
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Razor Pages (UI)
builder.Services.AddRazorPages();

// Dependency Injection
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();

var app = builder.Build();

//
// ================= MIDDLEWARE PIPELINE =================
//

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 🚫 DO NOT USE HTTPS REDIRECTION ON RENDER FREE
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//
// ================= ROUTING =================
//

// Make root "/" open Dashboard directly
app.MapGet("/", context =>
{
    context.Response.Redirect("/Dashboard");
    return Task.CompletedTask;
});

app.MapControllers();
app.MapRazorPages();

app.Run();
