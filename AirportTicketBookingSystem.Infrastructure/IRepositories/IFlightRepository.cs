using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Infrastructure.IRepositories;

public interface IFlightRepository
{
    IEnumerable<Flight> GetAll();
    Flight? GetById(string id);
    void Add(Flight flight);
    void AddRange(IEnumerable<Flight> flights);
    void Delete(string id);
}