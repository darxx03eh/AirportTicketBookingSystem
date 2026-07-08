using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Tests.Utils.Helpers;

public class PassengerServiceTestHelper
{
    public static Passenger MakePassenger(string id = "P1") => new()
    {
        Id = id,
        FirstName = "Mahmoud",
        LastName = "Darawsheh",
        Email = "mahmoud@example.com"
    };
}