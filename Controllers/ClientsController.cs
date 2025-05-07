using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;
    private readonly ITripsService _tripsService;

    public ClientsController(IClientsService clientsService, ITripsService tripsService)
    {
        _clientsService = clientsService;
        _tripsService = tripsService;
    }

    [HttpPost]
    public async Task<IActionResult> AddClient([FromBody] ClientDTO dto)
    {
        var id = await _clientsService.AddClientAsync(dto);
        return CreatedAtAction(nameof(AddClient), new { id = id }, dto);
    }

    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        if (!await _clientsService.DoesClientExistAsync(id))
            return NotFound("Client not found.");

        var trips = await _clientsService.GetTripsForClientAsync(id);
        return Ok(trips);
    }

    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> EnrollClientToTrip(int id, int tripId)
    {
        if (!await _clientsService.DoesClientExistAsync(id))
            return NotFound("Client not found.");

        if (!await _tripsService.DoesTripExistAsync(tripId))
            return NotFound("Trip not found.");

        if (await _clientsService.IsClientEnrolledInTripAsync(id, tripId))
            return Conflict("Client already enrolled.");

        var success = await _clientsService.EnrollClientToTripAsync(id, tripId);
        
        if (!success)
            return BadRequest("Enrollment failed.");

        return Ok("Client enrolled successfully.");
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> RemoveClientFromTrip(int id, int tripId)
    {
        var removed = await _clientsService.RemoveClientFromTripAsync(id, tripId);
        if (!removed)
            return NotFound("Client not enrolled in this trip.");

        return Ok("Client removed from trip.");
    }
}
