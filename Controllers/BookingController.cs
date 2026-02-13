using Microsoft.AspNetCore.Mvc;
using BookingSystemAPI.Models;
using BookingSystemAPI.Services;

namespace BookingSystemAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BookingController : ControllerBase
	{
		private readonly IBookingService _service;

		public BookingController(IBookingService service)
		{
			_service = service;
		}

		// ================= CRUD =================
		[HttpGet]
		public async Task<ActionResult<List<Booking>>> GetAll()
		{
			return Ok(await _service.GetAllAsync());
		}

		[HttpGet("{id:length(24)}")]
		public async Task<ActionResult<Booking>> GetById(string id)
		{
			var booking = await _service.GetByIdAsync(id);
			if (booking == null)
				return NotFound("Booking not found");

			return Ok(booking);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] Booking booking)
		{
			await _service.CreateAsync(booking);
			return Ok(new { message = "Booking created successfully" });
		}

		[HttpPut("{id:length(24)}")]
		public async Task<IActionResult> Update(string id, [FromBody] Booking booking)
		{
			var existing = await _service.GetByIdAsync(id);
			if (existing == null)
				return NotFound("Booking not found");

			await _service.UpdateAsync(id, booking);
			return Ok(new { message = "Booking updated successfully" });
		}

		[HttpPatch("{id:length(24)}")]
		public async Task<IActionResult> Edit(string id, [FromBody] Booking booking)
		{
			var existing = await _service.GetByIdAsync(id);
			if (existing == null)
				return NotFound("Booking not found");

			await _service.UpdateAsync(id, booking);
			return Ok(new { message = "Booking edited successfully" });
		}

		[HttpDelete("{id:length(24)}")]
		public async Task<IActionResult> Delete(string id)
		{
			var existing = await _service.GetByIdAsync(id);
			if (existing == null)
				return NotFound("Booking not found");

			await _service.DeleteAsync(id);
			return Ok(new { message = "Booking deleted successfully" });
		}

		// ================= Filters =================
		[HttpGet("by-status")]
		public async Task<ActionResult<List<Booking>>> ByStatus([FromQuery] string status)
		{
			return Ok(await _service.GetByStatusAsync(status));
		}

		[HttpGet("by-hotel")]
		public async Task<ActionResult<List<Booking>>> ByHotel([FromQuery] string hotel)
		{
			return Ok(await _service.GetByHotelAsync(hotel));
		}

		[HttpGet("by-date")]
		public async Task<ActionResult<List<Booking>>> ByDate([FromQuery] string from, [FromQuery] string to)
		{
			return Ok(await _service.GetByDateRangeAsync(from, to));
		}

		[HttpGet("by-day")]
		public async Task<ActionResult<List<Booking>>> ByDay([FromQuery] string date)
		{
			return Ok(await _service.GetByParticularDayAsync(date));
		}

		[HttpGet("upcoming")]
		public async Task<ActionResult<List<Booking>>> Upcoming()
		{
			return Ok(await _service.GetUpcomingCheckInsAsync());
		}

		[HttpGet("paged")]
		public async Task<ActionResult<List<Booking>>> Paged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			return Ok(await _service.GetPaginatedAsync(page, pageSize));
		}

		[HttpGet("filter")]
		public async Task<ActionResult<List<Booking>>> Filter([FromQuery] string? status, [FromQuery] string? hotel, [FromQuery] string? from, [FromQuery] string? to)
		{
			return Ok(await _service.GetCombinedFilterAsync(status, hotel, from, to));
		}

		// ================= Analytics =================
		[HttpGet("status-count")]
		public async Task<ActionResult<List<StatusCount>>> StatusCount()
		{
			return Ok(await _service.GetStatusCountAsync());
		}

		[HttpGet("hotel-count")]
		public async Task<ActionResult<List<StatusCount>>> HotelCount()
		{
			return Ok(await _service.GetHotelWiseCountAsync());
		}

		[HttpGet("trend")]
		public async Task<ActionResult<List<TrendData>>> Trend()
		{
			return Ok(await _service.GetBookingTrendAsync());
		}

		[HttpGet("dashboard-summary")]
		public async Task<ActionResult<DashboardSummary>> DashboardSummary()
		{
			return Ok(await _service.GetDashboardSummaryAsync());
		}

		[HttpGet("dashboard-data")]
		public async Task<ActionResult<DashboardData>> DashboardData()
		{
			return Ok(await _service.GetDashboardDataAsync());
		}

		// ================= Utility =================
		[HttpGet("generate")]
		public async Task<IActionResult> Generate([FromQuery] int count = 50)
		{
			await _service.GenerateRandomBookingsAsync(count);
			return Ok(new { message = $"{count} random bookings inserted successfully" });
		}
	}
}
