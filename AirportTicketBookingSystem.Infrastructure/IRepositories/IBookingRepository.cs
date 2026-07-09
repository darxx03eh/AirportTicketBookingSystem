using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Infrastructure.IRepositories;

public interface IBookingRepository
{
    IEnumerable<Booking> GetAll();
    Booking? GetById(string id);
    IEnumerable<Booking> GetByPassengerId(string passengerId);
    void Add(Booking booking);
    void Update(Booking booking);
    void Delete(string id);
}