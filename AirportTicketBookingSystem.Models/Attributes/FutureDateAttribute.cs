using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Models.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute() : base("The {0} must be a future date.") {}
    public override bool IsValid(object? value)
    {
        if (value is DateTime date)
            return date.Date >= DateTime.Today;
        return false;
    }

    public string GetConstraintDescription() => "Required, Allowed Range (today → future)";
}