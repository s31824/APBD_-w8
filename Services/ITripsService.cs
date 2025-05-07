namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTripsAsync();
    Task<bool> DoesTripExistAsync(int idTrip);
}