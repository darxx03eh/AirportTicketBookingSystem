using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;

namespace AirportTicketBookingSystem.Tests.Utils.Helpers;

public static class BookingServiceTestHelpers
{
    public static Flight MakeFlight(string id = "F1") => new()
    {
        Id = id,
        FlightNumber = "AB123",
        DepartureCountry = "USA",
        DestinationCountry = "UK",
        DepartureAirport = "JFK",
        ArrivalAirport = "LHR",
        DepartureDate = new DateTime(2026, 8, 1)
    };

    public static Passenger MakePassenger(string id = "P1") => new()
    {
        Id = id,
        FirstName = "Mahmoud",
        LastName = "Darawsheh"
    };

    public static Booking MakeBooking(string id = "B1", string flightId = "F1", string passengerId = "P1")
        => new()
        {
            Id = id,
            FlightId = flightId,
            PassengerId = passengerId,
            Type = FlightType.Economy,
        };
}