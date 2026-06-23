using AirportTicketBookingSystem.Models.Enums;

namespace AirportTicketBookingSystem.Service.DTOs;

public class FlightSearchFilter
{
    public decimal? MaxPrice { get; set; }
    public string? DepartureCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public string? DepartureAirport { get; set; }
    public string? ArrivalAirport { get; set; }
    public DateTime? DepartureDate { get; set; }
    public FlightType? Type { get; set; }
}