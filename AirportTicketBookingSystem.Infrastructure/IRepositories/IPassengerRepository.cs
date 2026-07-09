using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Infrastructure.IRepositories;

public interface IPassengerRepository
{
    IEnumerable<Passenger> GetAll();
    Passenger? GetById(string id);
    Passenger? GetByEmail(string email);
    void Add(Passenger passenger);
}