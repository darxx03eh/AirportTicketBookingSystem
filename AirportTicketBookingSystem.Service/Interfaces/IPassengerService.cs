using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Service.Interfaces;

public interface IPassengerService
{
    Passenger GetOrCreate(string firstName, string lastName, string email);
    Passenger? GetById(string id);
}