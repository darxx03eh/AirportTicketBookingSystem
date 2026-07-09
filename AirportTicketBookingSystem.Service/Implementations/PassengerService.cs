using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Service.Interfaces;

namespace AirportTicketBookingSystem.Service.Implementations;

public class PassengerService(IPassengerRepository passengerRepository) : IPassengerService
{
    private readonly IPassengerRepository _passengerRepository = passengerRepository;
    public Passenger GetOrCreate(string? firstName, string? lastName, string? email)
    {
        var existing = _passengerRepository.GetByEmail(email);
        if (existing != null) return existing;

        var passenger = new Passenger
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        _passengerRepository.Add(passenger);
        return passenger;
    }

    public Passenger? GetById(string id) => _passengerRepository.GetById(id);
}