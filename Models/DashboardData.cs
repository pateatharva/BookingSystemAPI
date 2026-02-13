using System.Collections.Generic;

namespace BookingSystemAPI.Models
{
	public class DashboardData
	{
		public DashboardSummary Summary { get; set; }
		public List<StatusCount> StatusChart { get; set; }
		public List<StatusCount> HotelChart { get; set; }
		public List<TrendData> TrendChart { get; set; }
	}
}
