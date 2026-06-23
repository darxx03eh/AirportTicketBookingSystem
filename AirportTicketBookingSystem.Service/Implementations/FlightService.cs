using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AirportTicketBookingSystem.Infrastructure.IParsers;
using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Attributes;
using AirportTicketBookingSystem.Models.DTOs;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Service.DTOs;
using AirportTicketBookingSystem.Service.Interfaces;

namespace AirportTicketBookingSystem.Service.Implementations;

public class FlightService(
    IFlightRepository flightRepository,
    IFlightCsvParser csvParser
    ) : IFlightService
{
    private readonly IFlightRepository _flightRepository = flightRepository;
    private readonly IFlightCsvParser _csvParser = csvParser;
    public IEnumerable<Flight> SearchFlights(FlightSearchFilter filter)
    {
        var flights = _flightRepository.GetAll();

        if (!string.IsNullOrWhiteSpace(filter.DepartureCountry))
            flights = flights.Where(f => f.DepartureCountry.Contains(filter.DepartureCountry, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.DestinationCountry))
            flights = flights.Where(f => f.DestinationCountry.Contains(filter.DestinationCountry, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.DepartureAirport))
            flights = flights.Where(f => f.DepartureAirport.Contains(filter.DepartureAirport, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.ArrivalAirport))
            flights = flights.Where(f => f.ArrivalAirport.Contains(filter.ArrivalAirport, StringComparison.OrdinalIgnoreCase));

        if (filter.DepartureDate.HasValue)
            flights = flights.Where(f => f.DepartureDate.Date == filter.DepartureDate.Value.Date);

        if (filter.MaxPrice.HasValue && filter.Type.HasValue)
            flights = flights.Where(f => f.GetPriceForType(filter.Type.Value) <= filter.MaxPrice.Value);

        return flights.OrderBy(f => f.DepartureDate);
    }

    public Flight? GetById(string id) => _flightRepository.GetById(id);

    public ImportResult ImportFromCsv(string csvFilePath)
    {
        var result = new ImportResult();
        var (flights, errors) = _csvParser.Parse(csvFilePath);

        foreach (var (flight, rowNum) in flights)
        {
            var validationErrors = ValidateFlight(flight, rowNum);
            if (validationErrors.Messages.Count > 0)
                result.Errors.Add(validationErrors);
            else
                result.SuccessfulFlights.Add(flight);
        }

        result.Errors.AddRange(errors);

        if (result.SuccessfulFlights.Count > 0)
            _flightRepository.AddRange(result.SuccessfulFlights);

        return result;
    }

    private ImportError ValidateFlight(Flight flight, int rowNumber)
    {
        var error = new ImportError { RowNumber = rowNumber };
        var context = new ValidationContext(flight);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(flight, context, results, validateAllProperties: true))
            error.Messages.AddRange(results.Select(r => r.ErrorMessage ?? "Unknown error"));

        return error;
    }

    public IEnumerable<ValidationFieldInfo> GetFlightValidationDetails()
    {
        var details = new List<ValidationFieldInfo>();
        var properties = typeof(Flight).GetProperties();

        foreach (var prop in properties)
        {
            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr == null) continue;

            var fieldInfo = new ValidationFieldInfo
            {
                FieldName = displayAttr.Name ?? prop.Name,
                Type = displayAttr.Description ?? GetTypeName(prop.PropertyType),
                Constraints = BuildConstraints(prop)
            };

            details.Add(fieldInfo);
        }

        return details;
    }

    private string BuildConstraints(PropertyInfo prop)
    {
        var parts = new List<string>();

        if (prop.GetCustomAttribute<RequiredAttribute>() != null)
            parts.Add("Required");

        if (prop.GetCustomAttribute<FutureDateAttribute>() is { } futureAttr)
            parts.Add(futureAttr.GetConstraintDescription());

        if (prop.GetCustomAttribute<PositiveDecimalAttribute>() is { } posAttr)
            parts.Add(posAttr.GetConstraintDescription());

        if (prop.GetCustomAttribute<RegularExpressionAttribute>() is { } regexAttr)
            parts.Add($"Pattern: {regexAttr.Pattern}");

        if (prop.GetCustomAttribute<RangeAttribute>() is { } rangeAttr)
            parts.Add($"Range: {rangeAttr.Minimum} - {rangeAttr.Maximum}");

        return parts.Count > 0 ? string.Join(", ", parts) : "None";
    }

    private string GetTypeName(Type type)
    {
        if (type == typeof(string)) return "Free Text";
        if (type == typeof(DateTime) || type == typeof(DateTime?)) return "Date Time";
        if (type == typeof(decimal) || type == typeof(decimal?)) return "Decimal";
        return type.Name;
    }
}
