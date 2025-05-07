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
        const string query = @"
        SELECT 
            t.IdTrip, t.Name, t.DateFrom, t.DateTo, t.MaxPeople,
            c.Name AS CountryName
        FROM Trip t
        LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
        LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
        ORDER BY t.IdTrip
    ";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        await conn.OpenAsync();

        using var reader = await cmd.ExecuteReaderAsync();

        Dictionary<int, TripDTO> tripMap = new();

        while (await reader.ReadAsync())
        {
            int idTrip = reader.GetInt32(0);

            if (!tripMap.ContainsKey(idTrip))
            {
                tripMap[idTrip] = new TripDTO
                {
                    Id = idTrip,
                    Name = reader.GetString(1),
                    DateFrom = reader.GetDateTime(2),
                    DateTo = reader.GetDateTime(3),
                    MaxPeople = reader.GetInt32(4),
                    Countries = new List<string>()
                };
            }

            if (!reader.IsDBNull(5))
            {
                tripMap[idTrip].Countries.Add(reader.GetString(5));
            }
        }

        return tripMap.Values.ToList();
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