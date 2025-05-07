using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;

    public ClientsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<int> AddClientAsync(ClientDTO client)
    {
        const string query = @"
            INSERT INTO Client (FirstName, LastName, Email, Telephone)
            OUTPUT INSERTED.IdClient
            VALUES (@FirstName, @LastName, @Email, @Telephone);
        ";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
        cmd.Parameters.AddWithValue("@LastName", client.LastName);
        cmd.Parameters.AddWithValue("@Email", client.Email ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Telephone", client.Telephone ?? (object)DBNull.Value);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> DoesClientExistAsync(int idClient)
    {
        const string query = "SELECT 1 FROM Client WHERE IdClient = @IdClient";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdClient", idClient);
        await conn.OpenAsync();

        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task<bool> IsClientEnrolledInTripAsync(int idClient, int idTrip)
    {
        const string query = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdClient", idClient);
        cmd.Parameters.AddWithValue("@IdTrip", idTrip);
        await conn.OpenAsync();

        return await cmd.ExecuteScalarAsync() is not null;
    }

    public async Task<bool> EnrollClientToTripAsync(int clientId, int tripId)
    {
        var query = @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
                  VALUES (@clientId, @tripId, @registeredAt, @paymentDate)";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(query, connection))
        {
            // Przygotuj INT-y w formacie YYYYMMDD
            int registeredAt = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            command.Parameters.AddWithValue("@clientId", clientId);
            command.Parameters.AddWithValue("@tripId", tripId);
            command.Parameters.AddWithValue("@registeredAt", registeredAt);
            command.Parameters.AddWithValue("@paymentDate", DBNull.Value); 

            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
    }


    public async Task<bool> RemoveClientFromTripAsync(int idClient, int idTrip)
    {
        const string query = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdClient", idClient);
        cmd.Parameters.AddWithValue("@IdTrip", idTrip);
        await conn.OpenAsync();

        var affectedRows = await cmd.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }

    public async Task<List<TripDTO>> GetTripsForClientAsync(int idClient)
    {
        var trips = new List<TripDTO>();
        const string query = @"
            SELECT t.IdTrip, t.Name, t.DateFrom, t.DateTo, t.maxPeople
            FROM Trip t
            JOIN Client_Trip ct ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @IdClient
        ";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@IdClient", idClient);
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
                MaxPeople = reader.GetInt32(4),
            });
        }

        return trips;
    }
    
}
