using AirportTicketBookingSystem.Infrastructure.IParsers;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Service.DTOs;

namespace AirportTicketBookingSystem.Infrastructure.Parsers;

public class FlightCsvParser : IFlightCsvParser
{
    private const string ExpectedHeader =
        "FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice";

    public (List<(Flight flight, int rowNumber)> Flights, List<ImportError> ParseErrors) Parse(string filePath)
    {
        var flights = new List<(Flight, int)>();
        var errors = new List<ImportError>();

        if (!File.Exists(filePath))
        {
            errors.Add(new ImportError { RowNumber = 0, Messages = { $"File not found: {filePath}" } });
            return (flights, errors);
        }

        var lines = File.ReadAllLines(filePath);

        if (lines.Length == 0)
        {
            errors.Add(new ImportError { RowNumber = 0, Messages = { "CSV file is empty." } });
            return (flights, errors);
        }

        // Skip header row
        foreach (var (line, index) in lines.Skip(1).Select((l, i) => (l, i + 2)))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parseResult = ParseLine(line, index);
            if (parseResult.error != null)
                errors.Add(parseResult.error);
            else if (parseResult.flight != null)
                flights.Add((parseResult.flight, index));
        }

        return (flights, errors);
    }

    private (Flight? flight, ImportError? error) ParseLine(string line, int rowNumber)
    {
        var columns = line.Split(',');
        var errorMessages = new List<string>();

        if (columns.Length < 7)
        {
            return (null, new ImportError
            {
                RowNumber = rowNumber,
                Messages = { $"Expected 7 columns, but found {columns.Length}." }
            });
        }

        DateTime departureDate = default;
        decimal basePrice = 0;

        if (!DateTime.TryParse(columns[5].Trim(), out departureDate))
            errorMessages.Add("Departure date has an invalid format. Expected: YYYY-MM-DD.");

        if (!decimal.TryParse(columns[6].Trim(), out basePrice))
            errorMessages.Add("Base price has an invalid format. Expected a decimal number.");

        if (errorMessages.Count > 0)
            return (null, new ImportError { RowNumber = rowNumber, Messages = errorMessages });

        var flight = new Flight
        {
            FlightNumber = columns[0].Trim(),
            DepartureCountry = columns[1].Trim(),
            DestinationCountry = columns[2].Trim(),
            DepartureAirport = columns[3].Trim(),
            ArrivalAirport = columns[4].Trim(),
            DepartureDate = departureDate,
            BasePrice = basePrice
        };

        return (flight, null);
    }
}