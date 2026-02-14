using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BookingSystemAPI.Models
{
	public class Booking
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("bookingId")]
		public string? BookingId { get; set; }

		[BsonElement("customerName")]
		public string? CustomerName { get; set; }

		[BsonElement("hotelName")]
		public string? HotelName { get; set; }

		[BsonElement("status")]
		public string? Status { get; set; }

		[BsonElement("checkInDate")]
		public string? CheckInDate { get; set; }

		[BsonElement("checkOutDate")]
		public string? CheckOutDate { get; set; }

		[BsonElement("createdAt")]
		public string? CreatedAt { get; set; }
	}
}
