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
