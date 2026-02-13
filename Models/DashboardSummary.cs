namespace BookingSystemAPI.Models
{
	public class DashboardSummary
	{
		public int TotalBookings { get; set; }
		public int Confirmed { get; set; }
		public int Pending { get; set; }
		public int Cancelled { get; set; }
	}
}
