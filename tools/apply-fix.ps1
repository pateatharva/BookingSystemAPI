# PowerShell script to overwrite project files with corrected implementations.
# Run from project root. Backup recommended (git stash / commit).

$files = @{
"Program.cs" = @'
using BookingSystemAPI.Repositories;
using BookingSystemAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // ensure camelCase JSON for frontend JS
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddRazorPages();

// MongoDbService as a singleton wrapper over IMongoClient/collections (thread-safe)
builder.Services.AddSingleton<MongoDbService>();

// Repository and Service are scoped (per HTTP request)
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Dev helpers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
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
'@

"Controllers\BookingController.cs" = @'
using Microsoft.AspNetCore.Mvc;
using BookingSystemAPI.Models;
using BookingSystemAPI.Services;

namespace BookingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // ================= CRUD =================
        [HttpGet]
        public async Task<ActionResult<List<Booking>>> Get()
            => Ok(await _bookingService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking?>> GetById(string id)
        {
            var item = await _bookingService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Booking booking)
        {
            await _bookingService.CreateAsync(booking);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Booking booking)
        {
            await _bookingService.UpdateAsync(id, booking);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _bookingService.DeleteAsync(id);
            return NoContent();
        }

        // ================= Filters =================
        [HttpGet("by-status")]
        public async Task<ActionResult<List<Booking>>> ByStatus([FromQuery] string status)
            => Ok(await _bookingService.GetByStatusAsync(status));

        [HttpGet("by-hotel")]
        public async Task<ActionResult<List<Booking>>> ByHotel([FromQuery] string hotel)
            => Ok(await _bookingService.GetByHotelAsync(hotel));

        [HttpGet("by-date")]
        public async Task<ActionResult<List<Booking>>> ByDate([FromQuery] string from, [FromQuery] string to)
            => Ok(await _bookingService.GetByDateRangeAsync(from, to));

        [HttpGet("filter")]
        public async Task<ActionResult<List<Booking>>> CombinedFilter([FromQuery] string? status, [FromQuery] string? hotel, [FromQuery] string? from, [FromQuery] string? to)
            => Ok(await _bookingService.GetCombinedFilterAsync(status, hotel, from, to));

        [HttpGet("by-day")]
        public async Task<ActionResult<List<Booking>>> ByDay([FromQuery] string date)
            => Ok(await _bookingService.GetByParticularDayAsync(date));

        [HttpGet("upcoming")]
        public async Task<ActionResult<List<Booking>>> Upcoming()
            => Ok(await _bookingService.GetUpcomingCheckInsAsync());

        [HttpGet("paged")]
        public async Task<ActionResult<List<Booking>>> Paged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            => Ok(await _bookingService.GetPaginatedAsync(page, pageSize));

        // ================= Analytics =================
        [HttpGet("status-count")]
        public async Task<ActionResult<List<StatusCount>>> StatusCount()
            => Ok(await _bookingService.GetStatusCountAsync());

        [HttpGet("hotel-count")]
        public async Task<ActionResult<List<StatusCount>>> HotelCount()
            => Ok(await _bookingService.GetHotelWiseCountAsync());

        [HttpGet("trend")]
        public async Task<ActionResult<List<TrendData>>> Trend()
            => Ok(await _bookingService.GetBookingTrendAsync());

        [HttpGet("dashboard-summary")]
        public async Task<ActionResult<DashboardSummary>> DashboardSummary()
            => Ok(await _bookingService.GetDashboardSummaryAsync());

        [HttpGet("dashboard-data")]
        public async Task<ActionResult<DashboardData>> DashboardData()
            => Ok(await _bookingService.GetDashboardDataAsync());

        // ================= Utility =================
        [HttpGet("generate")]
        public async Task<IActionResult> Generate([FromQuery] int count = 50)
        {
            await _bookingService.GenerateRandomBookingsAsync(count);
            return Ok(new { message = $"{count} random bookings inserted successfully" });
        }
    }
}
'@

"Services\IBookingService.cs" = @'
using BookingSystemAPI.Models;

namespace BookingSystemAPI.Services
{
    public interface IBookingService
    {
        // CRUD
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(string id);
        Task CreateAsync(Booking booking);
        Task UpdateAsync(string id, Booking booking);
        Task DeleteAsync(string id);

        // Filters
        Task<List<Booking>> GetByStatusAsync(string status);
        Task<List<Booking>> GetByHotelAsync(string hotel);
        Task<List<Booking>> GetByDateRangeAsync(string from, string to);
        Task<List<Booking>> GetUpcomingCheckInsAsync();
        Task<List<Booking>> GetByParticularDayAsync(string date);
        Task<List<Booking>> GetPaginatedAsync(int page, int pageSize);
        Task<List<Booking>> GetCombinedFilterAsync(string? status, string? hotel, string? from, string? to);

        // Analytics
        Task<List<StatusCount>> GetStatusCountAsync();
        Task<List<StatusCount>> GetHotelWiseCountAsync();
        Task<List<TrendData>> GetBookingTrendAsync();
        Task<DashboardSummary> GetDashboardSummaryAsync();
        Task<DashboardData> GetDashboardDataAsync();

        // Utilities
        Task GenerateRandomBookingsAsync(int count);
    }
}
'@

"Services\BookingService.cs" = @'
using BookingSystemAPI.Models;
using BookingSystemAPI.Repositories;

namespace BookingSystemAPI.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;

        public BookingService(IBookingRepository repository)
        {
            _repository = repository;
        }

        // CRUD
        public async Task<List<Booking>> GetAllAsync()
            => await _repository.GetAllAsync();

        public async Task<Booking?> GetByIdAsync(string id)
            => await _repository.GetByIdAsync(id);

        public async Task CreateAsync(Booking booking)
        {
            booking.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd");
            await _repository.CreateAsync(booking);
        }

        public async Task UpdateAsync(string id, Booking booking)
        {
            booking.Id = id;
            await _repository.UpdateAsync(id, booking);
        }

        public async Task DeleteAsync(string id)
            => await _repository.DeleteAsync(id);

        // Filters
        public async Task<List<Booking>> GetByStatusAsync(string status)
            => await _repository.GetByStatusAsync(status);

        public async Task<List<Booking>> GetByHotelAsync(string hotel)
            => await _repository.GetByHotelAsync(hotel);

        public async Task<List<Booking>> GetByDateRangeAsync(string from, string to)
            => await _repository.GetByDateRangeAsync(from, to);

        public async Task<List<Booking>> GetUpcomingCheckInsAsync()
            => await _repository.GetUpcomingCheckInsAsync();

        public async Task<List<Booking>> GetByParticularDayAsync(string date)
            => await _repository.GetByParticularDayAsync(date);

        public async Task<List<Booking>> GetPaginatedAsync(int page, int pageSize)
            => await _repository.GetPaginatedAsync(page, pageSize);

        public async Task<List<Booking>> GetCombinedFilterAsync(string? status, string? hotel, string? from, string? to)
            => await _repository.GetCombinedFilterAsync(status, hotel, from, to);

        // Analytics
        public async Task<List<StatusCount>> GetStatusCountAsync()
            => await _repository.GetStatusCountAsync();

        public async Task<List<StatusCount>> GetHotelWiseCountAsync()
            => await _repository.GetHotelWiseCountAsync();

        public async Task<List<TrendData>> GetBookingTrendAsync()
            => await _repository.GetBookingTrendAsync();

        public async Task<DashboardSummary> GetDashboardSummaryAsync()
            => await _repository.GetDashboardSummaryAsync();

        public async Task<DashboardData> GetDashboardDataAsync()
            => await _repository.GetDashboardDataAsync();

        // Utilities
        public async Task GenerateRandomBookingsAsync(int count)
            => await _repository.GenerateRandomBookingsAsync(count);
    }
}
'@

"Repositories\IBookingRepository.cs" = @'
using BookingSystemAPI.Models;

namespace BookingSystemAPI.Repositories
{
    public interface IBookingRepository
    {
        // CRUD
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(string id);
        Task CreateAsync(Booking booking);
        Task UpdateAsync(string id, Booking booking);
        Task DeleteAsync(string id);

        // Filters
        Task<List<Booking>> GetByStatusAsync(string status);
        Task<List<Booking>> GetByHotelAsync(string hotel);
        Task<List<Booking>> GetByDateRangeAsync(string from, string to);
        Task<List<Booking>> GetUpcomingCheckInsAsync();
        Task<List<Booking>> GetByParticularDayAsync(string date);
        Task<List<Booking>> GetPaginatedAsync(int page, int pageSize);
        Task<List<Booking>> GetCombinedFilterAsync(string? status, string? hotel, string? from, string? to);

        // Analytics
        Task<List<StatusCount>> GetStatusCountAsync();
        Task<List<StatusCount>> GetHotelWiseCountAsync();
        Task<List<TrendData>> GetBookingTrendAsync();
        Task<DashboardSummary> GetDashboardSummaryAsync();
        Task<DashboardData> GetDashboardDataAsync();

        // Utilities
        Task GenerateRandomBookingsAsync(int count);
    }
}
'@

"Repositories\BookingRepository.cs" = @'
using BookingSystemAPI.Models;
using BookingSystemAPI.Services;

namespace BookingSystemAPI.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly MongoDbService _mongoDbService;

        public BookingRepository(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // CRUD
        public async Task<List<Booking>> GetAllAsync()
            => await _mongoDbService.GetAllAsync();

        public async Task<Booking?> GetByIdAsync(string id)
            => await _mongoDbService.GetByIdAsync(id);

        public async Task CreateAsync(Booking booking)
            => await _mongoDbService.CreateAsync(booking);

        public async Task UpdateAsync(string id, Booking booking)
            => await _mongoDbService.UpdateAsync(id, booking);

        public async Task DeleteAsync(string id)
            => await _mongoDbService.DeleteAsync(id);

        // Filters
        public async Task<List<Booking>> GetByStatusAsync(string status)
            => await _mongoDbService.GetByStatusAsync(status);

        public async Task<List<Booking>> GetByHotelAsync(string hotel)
            => await _mongoDbService.GetByHotelAsync(hotel);

        public async Task<List<Booking>> GetByDateRangeAsync(string from, string to)
            => await _mongoDbService.GetByDateRangeAsync(from, to);

        public async Task<List<Booking>> GetUpcomingCheckInsAsync()
            => await _mongoDbService.GetUpcomingCheckInsAsync();

        public async Task<List<Booking>> GetByParticularDayAsync(string date)
            => await _mongoDbService.GetByParticularDayAsync(date);

        public async Task<List<Booking>> GetPaginatedAsync(int page, int pageSize)
            => await _mongoDbService.GetPaginatedAsync(page, pageSize);

        public async Task<List<Booking>> GetCombinedFilterAsync(string? status, string? hotel, string? from, string? to)
            => await _mongoDbService.GetCombinedFilterAsync(status, hotel, from, to);

        // Analytics
        public async Task<List<StatusCount>> GetStatusCountAsync()
            => await _mongoDbService.GetStatusCountAsync();

        public async Task<List<StatusCount>> GetHotelWiseCountAsync()
            => await _mongoDbService.GetHotelWiseCountAsync();

        public async Task<List<TrendData>> GetBookingTrendAsync()
            => await _mongoDbService.GetBookingTrendAsync();

        public async Task<DashboardSummary> GetDashboardSummaryAsync()
            => await _mongoDbService.GetDashboardSummaryAsync();

        public async Task<DashboardData> GetDashboardDataAsync()
            => await _mongoDbService.GetDashboardDataAsync();

        // Utilities
        public async Task GenerateRandomBookingsAsync(int count)
            => await _mongoDbService.GenerateRandomBookingsAsync(count);
    }
}
'@
}

# Create directories and write files
foreach ($path in $files.Keys) {
    $dir = Split-Path $path
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created directory: $dir"
    }
    $content = $files[$path]
    $fullPath = Join-Path (Get-Location) $path
    $content | Set-Content -Path $fullPath -Encoding UTF8
    Write-Host "Wrote file: $path"
}

Write-Host "Done. Please rebuild the solution and ensure appsettings.json contains MongoDbSettings (ConnectionString, DatabaseName, CollectionName)."
Write-Host "Run the project (F5) and test endpoints such as: https://localhost:7001/api/booking/dashboard-data or http://localhost:5147/api/booking/dashboard-data (use your actual port)."