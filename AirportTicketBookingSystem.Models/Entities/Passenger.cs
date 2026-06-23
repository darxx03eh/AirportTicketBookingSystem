using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Models.Entities;

public class Passenger
{
    public string Id { get; set; } =  Guid.NewGuid().ToString();
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}