namespace AirportTicketBookingSystem.Service.DTOs;

public class ValidationFieldInfo
{
    public string FieldName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Constraints { get; set; } = string.Empty;
}