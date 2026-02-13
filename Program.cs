using System.Text.Json;
using BookingSystemAPI.Repositories;
using BookingSystemAPI.Services;

var builder = WebApplication.CreateBuilder(args);

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

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
