using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<int> AddClientAsync(ClientDTO client);
    Task<bool> DoesClientExistAsync(int idClient);
    Task<bool> IsClientEnrolledInTripAsync(int idClient, int idTrip);
    Task<bool> EnrollClientToTripAsync(int idClient, int idTrip);
    Task<bool> RemoveClientFromTripAsync(int idClient, int idTrip);
    Task<List<TripDTO>> GetTripsForClientAsync(int idClient);
}