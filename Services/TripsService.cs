using Microsoft.Data.SqlClient;
namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString;

    public TripsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<List<TripDTO>> GetTripsAsync()
    {
        var trips = new List<TripDTO>();
        var query = "SELECT IdTrip, Name, DateFrom, DateTo, maxPeople FROM Trip";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(new TripDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                DateFrom = reader.GetDateTime(2),
                DateTo = reader.GetDateTime(3),
                MaxPeople = reader.GetInt32(4)
            });
        }

        return trips;
    }

    public async Task<bool> DoesTripExistAsync(int idTrip)
    {
        var query = "SELECT 1 FROM Trip WHERE IdTrip = @IdTrip";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdTrip", idTrip);
        await conn.OpenAsync();

        return await cmd.ExecuteScalarAsync() is not null;
    }
}