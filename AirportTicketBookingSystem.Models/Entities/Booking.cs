using AirportTicketBookingSystem.Models.Enums;

namespace AirportTicketBookingSystem.Models.Entities;

public class Booking
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FlightId { get; set; } = string.Empty;
    public Flight? Flight { get; set; }
    public string PassengerId { get; set; } = string.Empty;
    public Passenger?  Passenger { get; set; }
    public FlightType Type { get; set; }
    public decimal TotalPrice => Flight?.GetPriceForType(Type) ?? 0;
    public DateTime BookedAt { get; set; } = DateTime.Now;
}