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
