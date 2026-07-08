using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Tests.Utils.Helpers;

public class FlightServiceTestHelpers
{
    public static Flight MakeFlight(
        string id = "F1",
        string number = "AB123",
        string departureCountry = "USA",
        string destinationCountry = "UK",
        string departureAirport = "JFK",
        string arrivalAirport = "LHR",
        DateTime? departureDate = null
    ) => new()
    {
        Id = id,
        FlightNumber = number,
        DepartureCountry = departureCountry,
        DestinationCountry = destinationCountry,
        DepartureAirport = departureAirport,
        ArrivalAirport = arrivalAirport,
        DepartureDate = departureDate ?? new DateTime(2026, 8, 1)
    };

   public static Flight MakeParsedFlight(
       string number = "AB123",
       string departureCountry = "USA",
       string destinationCountry = "UK",
       string departureAirport = "JFK",
       string arrivalAirport = "LHR",
       DateTime? departureDate = null,
       decimal basePrice = 199.99m) => new()
   {
       FlightNumber = number,
       DepartureCountry = departureCountry,
       DestinationCountry = destinationCountry,
       DepartureAirport = departureAirport,
       ArrivalAirport = arrivalAirport,
       DepartureDate = departureDate ?? DateTime.Today.AddMonths(1),
       BasePrice = basePrice
   };
}