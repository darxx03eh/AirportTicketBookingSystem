namespace AirportTicketBookingSystem.Service.DTOs;

public class ImportError
{
    public int RowNumber { get; set; }
    public List<string> Messages { get; set; } = new();
}