using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Service.DTOs;

namespace AirportTicketBookingSystem.Service.Interfaces;

public interface IFlightService
{
    IEnumerable<Flight> SearchFlights(FlightSearchFilter filter);
    Flight? GetById(string id);
    ImportResult ImportFromCsv(string csvFilePath);
    IEnumerable<ValidationFieldInfo> GetFlightValidationDetails();
}