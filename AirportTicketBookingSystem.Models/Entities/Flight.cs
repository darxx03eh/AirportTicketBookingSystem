using System.ComponentModel.DataAnnotations;
using AirportTicketBookingSystem.Models.Attributes;
using AirportTicketBookingSystem.Models.Enums;

namespace AirportTicketBookingSystem.Models.Entities;

public class Flight : IEquatable<Flight>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Flight Name is required.")]
    [RegularExpression(@"^[A-Z]{2}\d{3,4}$", ErrorMessage = "Flight number must be like \'AB123\'.")]
    [Display(Name = "Flight Number", Description = "Format: 2 uppercase letters + 3-4 digits (e.g. AB123)")]
    public string FlightNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departure country is required.")]
    [Display(Name = "Departure Country", Description = "Free Text")]
    public string DepartureCountry { get; set; } = string.Empty;

    [Required(ErrorMessage = "Destination country is required.")]
    [Display(Name = "Destination Country", Description = "Free Text")]
    public string DestinationCountry { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departure airport is required.")]
    [Display(Name = "Departure Airport", Description = "Free Text")]
    public string DepartureAirport { get; set; } = string.Empty;

    [Required(ErrorMessage = "Arrival airport is required.")]
    [Display(Name = "Arrival Airport", Description = "Free Text")]
    public string ArrivalAirport { get; set; } = string.Empty;

    [Required(ErrorMessage = "Departure date is required.")]
    [FutureDate]
    [Display(Name = "Departure Date", Description = "Date Time")]
    public DateTime DepartureDate { get; set; }

    [Required(ErrorMessage = "Base price is required.")]
    [PositiveDecimal]
    [Display(Name = "Base Price (USD)", Description = "Decimal")]
    public decimal BasePrice { get; set; }

    public decimal GetPriceForType(FlightType type) => type switch
    {
        FlightType.Economy => BasePrice,
        FlightType.Business => BasePrice * 1.5m,
        FlightType.FirstClass => BasePrice * 2.0m,
        _ => BasePrice
    };

    public bool Equals(Flight? other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Flight);
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(Flight left, Flight right)
    {
        return EqualityComparer<Flight>.Default.Equals(left, right);
    }

    public static bool operator !=(Flight left, Flight right)
    {
        return !(left == right);
    }
}