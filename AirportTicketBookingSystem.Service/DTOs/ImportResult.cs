using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Service.DTOs;

public class ImportResult
{
    public List<Flight> SuccessfulFlights { get; set; } = new();
    public List<ImportError> Errors { get; set; } = new();
    public int TotalRows => SuccessfulFlights.Count + Errors.Count;
    public bool HasErrors => Errors.Count > 0;
}