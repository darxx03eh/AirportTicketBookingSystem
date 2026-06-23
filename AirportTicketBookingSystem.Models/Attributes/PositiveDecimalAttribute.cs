using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Models.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PositiveDecimalAttribute : ValidationAttribute
{
    public PositiveDecimalAttribute() : base("The {0} must be a positive number") {}
    public override bool IsValid(object? value)
    {
        if (value is decimal d)
            return d > 0;
        return false;
    }

    public string GetConstraintDescription() => "Required, Must be greater than 0";
}