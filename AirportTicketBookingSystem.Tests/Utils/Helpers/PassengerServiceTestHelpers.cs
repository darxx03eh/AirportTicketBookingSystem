using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Tests.Utils.Helpers;

public class PassengerServiceTestHelpers
{
    public static Passenger MakePassenger(string id = "P1", string firstName = "Mahmoud",
        string lastName = "Darawsheh", string email = "mahmoud@example.com") => new()
    {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        Email = email
    };
}