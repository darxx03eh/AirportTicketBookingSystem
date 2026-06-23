using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Service.DTOs;

namespace AirportTicketBookingSystem.Infrastructure.IParsers;

public interface IFlightCsvParser
{
    (List<(Flight flight, int rowNumber)> Flights, List<ImportError> ParseErrors) Parse(string path);
}