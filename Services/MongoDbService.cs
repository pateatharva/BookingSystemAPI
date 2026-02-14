using MongoDB.Driver;
using MongoDB.Bson;
using BookingSystemAPI.Models;

namespace BookingSystemAPI.Services
{
	public class MongoDbService
	{
		private readonly IMongoCollection<Booking> _bookingCollection;

		public MongoDbService(IConfiguration configuration)
		{
            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            var collectionName = configuration["MongoDbSettings:CollectionName"];
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _bookingCollection = database.GetCollection<Booking>(collectionName);
		}

		// ================= GET ALL BOOKINGS =================
		public async Task<List<Booking>> GetAllAsync()
		{
			return await _bookingCollection.Find(_ => true).ToListAsync();
		}

		// ================= GET BY ID =================
		public async Task<Booking?> GetByIdAsync(string id)
		{
			return await _bookingCollection.Find(b => b.Id == id).FirstOrDefaultAsync();
		}

		// ================= CREATE BOOKING =================
		public async Task CreateAsync(Booking booking)
		{
			await _bookingCollection.InsertOneAsync(booking);
		}

		// ================= UPDATE BOOKING =================
		public async Task UpdateAsync(string id, Booking updatedBooking)
		{
			await _bookingCollection.ReplaceOneAsync(b => b.Id == id, updatedBooking);
		}

		// ================= DELETE BOOKING =================
		public async Task DeleteAsync(string id)
		{
			await _bookingCollection.DeleteOneAsync(b => b.Id == id);
		}

		// ================= FILTER BY STATUS =================
		public async Task<List<Booking>> GetByStatusAsync(string status)
		{
			if (string.IsNullOrWhiteSpace(status))
				return new List<Booking>();

			var filter = Builders<Booking>.Filter.Regex(
				b => b.Status,
				new BsonRegularExpression(status, "i"));

			return await _bookingCollection.Find(filter).ToListAsync();
		}

		// ================= FILTER BY HOTEL =================
		public async Task<List<Booking>> GetByHotelAsync(string hotel)
		{
			if (string.IsNullOrWhiteSpace(hotel))
				return new List<Booking>();

			var filter = Builders<Booking>.Filter.Regex(
				b => b.HotelName,
				new BsonRegularExpression(hotel, "i"));

			return await _bookingCollection.Find(filter).ToListAsync();
		}

		// ================= FILTER BY DATE RANGE =================
		public async Task<List<Booking>> GetByDateRangeAsync(string from, string to)
		{
			var builder = Builders<Booking>.Filter;
			var filter = builder.Empty;

			if (!string.IsNullOrEmpty(from))
				filter &= builder.Gte(b => b.CreatedAt, from);

			if (!string.IsNullOrEmpty(to))
				filter &= builder.Lte(b => b.CreatedAt, to);

			return await _bookingCollection.Find(filter)
										   .SortBy(b => b.CreatedAt)
										   .ToListAsync();
		}

		// ================= UPCOMING CHECK-IN =================
		public async Task<List<Booking>> GetUpcomingCheckInsAsync()
		{
			var today = DateTime.Now.ToString("yyyy-MM-dd");

			var filter = Builders<Booking>.Filter.Gte(b => b.CheckInDate, today);

			return await _bookingCollection.Find(filter)
										   .SortBy(b => b.CheckInDate)
										   .ToListAsync();
		}

		// ================= PARTICULAR DAY BOOKINGS =================
		public async Task<List<Booking>> GetByParticularDayAsync(string date)
		{
			if (string.IsNullOrEmpty(date))
				return new List<Booking>();

			return await _bookingCollection
				.Find(b => b.CreatedAt == date)
				.ToListAsync();
		}

		// ================= PAGINATION =================
		public async Task<List<Booking>> GetPaginatedAsync(int page, int pageSize)
		{
			return await _bookingCollection
				.Find(_ => true)
				.Skip((page - 1) * pageSize)
				.Limit(pageSize)
				.ToListAsync();
		}

		// ================= COMBINED FILTER =================
		public async Task<List<Booking>> GetCombinedFilterAsync(
			string? status,
			string? hotel,
			string? from,
			string? to)
		{
			var builder = Builders<Booking>.Filter;
			var filter = builder.Empty;

			if (!string.IsNullOrWhiteSpace(status))
				filter &= builder.Regex(b => b.Status, new BsonRegularExpression(status, "i"));

			if (!string.IsNullOrWhiteSpace(hotel))
				filter &= builder.Regex(b => b.HotelName, new BsonRegularExpression(hotel, "i"));

			if (!string.IsNullOrWhiteSpace(from))
				filter &= builder.Gte(b => b.CreatedAt, from);

			if (!string.IsNullOrWhiteSpace(to))
				filter &= builder.Lte(b => b.CreatedAt, to);

			return await _bookingCollection.Find(filter)
										   .SortByDescending(b => b.CreatedAt)
										   .ToListAsync();
		}

			// ================= STATUS COUNT (FIXED - CORRECT NULL CHECK) =================
		public async Task<List<StatusCount>> GetStatusCountAsync()
		{
			try
			{
				var pipeline = new[]
				{
					new BsonDocument("$match", new BsonDocument("status", new BsonDocument("$exists", true))),
					new BsonDocument("$group",
						new BsonDocument
						{
							{ "_id", "$status" },
							{ "count", new BsonDocument("$sum", 1) }
						}),
					new BsonDocument("$sort", new BsonDocument("_id", 1))
				};

				var result = await _bookingCollection
					.Aggregate<BsonDocument>(pipeline)
					.ToListAsync();

				var statusCounts = new List<StatusCount>();
				foreach (var doc in result)
				{
					if (doc.Contains("_id") && doc["_id"].BsonType != BsonType.Null)
					{
						statusCounts.Add(new StatusCount
						{
							Status = doc["_id"].AsString,
							Count = doc["count"].AsInt32
						});
					}
				}

				return statusCounts;
			}
			catch
			{
				return new List<StatusCount>();
			}
		}

		// ================= HOTEL COUNT (FIXED - CORRECT NULL CHECK) =================
		public async Task<List<StatusCount>> GetHotelWiseCountAsync()
		{
			try
			{
				var pipeline = new[]
				{
					new BsonDocument("$match", new BsonDocument("hotelName", new BsonDocument("$exists", true))),
					new BsonDocument("$group",
						new BsonDocument
						{
							{ "_id", "$hotelName" },
							{ "count", new BsonDocument("$sum", 1) }
						}),
					new BsonDocument("$sort", new BsonDocument("_id", 1))
				};

				var result = await _bookingCollection
					.Aggregate<BsonDocument>(pipeline)
					.ToListAsync();

				var hotelCounts = new List<StatusCount>();
				foreach (var doc in result)
				{
					if (doc.Contains("_id") && doc["_id"].BsonType != BsonType.Null)
					{
						hotelCounts.Add(new StatusCount
						{
							Status = doc["_id"].AsString,
							Count = doc["count"].AsInt32
						});
					}
				}

				return hotelCounts;
			}
			catch
			{
				return new List<StatusCount>();
			}
		}

		// ================= TREND (FIXED - CORRECT NULL CHECK) =================
		public async Task<List<TrendData>> GetBookingTrendAsync()
		{
			try
			{
				var pipeline = new[]
				{
					new BsonDocument("$match", new BsonDocument("createdAt", new BsonDocument("$exists", true))),
					new BsonDocument("$group",
						new BsonDocument
						{
							{ "_id", "$createdAt" },
							{ "count", new BsonDocument("$sum", 1) }
						}),
					new BsonDocument("$sort", new BsonDocument("_id", 1))
				};

				var result = await _bookingCollection
					.Aggregate<BsonDocument>(pipeline)
					.ToListAsync();

				var trendData = new List<TrendData>();
				foreach (var doc in result)
				{
					if (doc.Contains("_id") && doc["_id"].BsonType != BsonType.Null)
					{
						trendData.Add(new TrendData
						{
							Date = doc["_id"].AsString,
							Count = doc["count"].AsInt32
						});
					}
				}

				return trendData.OrderBy(x => x.Date).ToList();
			}
			catch
			{
				return new List<TrendData>();
			}
		}

		// ================= DASHBOARD SUMMARY =================
		public async Task<DashboardSummary> GetDashboardSummaryAsync()
		{
			try
			{
				var total = await _bookingCollection.CountDocumentsAsync(_ => true);
				var confirmed = await _bookingCollection.CountDocumentsAsync(b => b.Status == "Confirmed");
				var pending = await _bookingCollection.CountDocumentsAsync(b => b.Status == "Pending");
				var cancelled = await _bookingCollection.CountDocumentsAsync(b => b.Status == "Cancelled");

				return new DashboardSummary
				{
					TotalBookings = (int)total,
					Confirmed = (int)confirmed,
					Pending = (int)pending,
					Cancelled = (int)cancelled
				};
			}
			catch
			{
				return new DashboardSummary { TotalBookings = 0, Confirmed = 0, Pending = 0, Cancelled = 0 };
			}
		}

		// ================= DASHBOARD DATA =================
		public async Task<DashboardData> GetDashboardDataAsync()
		{
			try
			{
				return new DashboardData
				{
					Summary = await GetDashboardSummaryAsync(),
					StatusChart = await GetStatusCountAsync(),
					HotelChart = await GetHotelWiseCountAsync(),
					TrendChart = await GetBookingTrendAsync()
				};
			}
			catch
			{
				return new DashboardData
				{
					Summary = new DashboardSummary { TotalBookings = 0, Confirmed = 0, Pending = 0, Cancelled = 0 },
					StatusChart = new List<StatusCount>(),
					HotelChart = new List<StatusCount>(),
					TrendChart = new List<TrendData>()
				};
			}
		}

		// ================= RANDOM DATA GENERATOR =================
		public async Task GenerateRandomBookingsAsync(int count)
		{
			var random = new Random();

			var hotels = new[] { "Hotel Taj", "Hotel Royal", "Hotel Grand", "Hotel Paradise" };
			var statuses = new[] { "Confirmed", "Pending", "Cancelled" };
			var names = new[] { "Rahul", "Amit", "Sneha", "Priya", "Arjun", "Meera", "Neha", "Rohit" };

			var bookings = new List<Booking>();

			for (int i = 0; i < count; i++)
			{
				var createdDate = DateTime.Now.AddDays(-random.Next(0, 90));

				bookings.Add(new Booking
				{
					BookingId = "B" + random.Next(1000, 9999),
					CustomerName = names[random.Next(names.Length)],
					HotelName = hotels[random.Next(hotels.Length)],
					Status = statuses[random.Next(statuses.Length)],
					CheckInDate = createdDate.AddDays(random.Next(1, 5)).ToString("yyyy-MM-dd"),
					CheckOutDate = createdDate.AddDays(random.Next(6, 10)).ToString("yyyy-MM-dd"),
					CreatedAt = createdDate.ToString("yyyy-MM-dd")
				});
			}

			if (bookings.Count > 0)
				await _bookingCollection.InsertManyAsync(bookings);
		}
	}
}
