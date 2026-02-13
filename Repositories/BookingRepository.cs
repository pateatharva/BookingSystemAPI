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
