using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Service.DTOs;
using AirportTicketBookingSystem.Service.Interfaces;
using AirportTicketBookingSystem.UI.Displays;
using AirportTicketBookingSystem.UI.IMenus;
using Spectre.Console;

namespace AirportTicketBookingSystem.UI.Menus;

public class ManagerMenu(
    IFlightService flightService,
    IBookingService bookingService
    ) : IManagerMenu
{
    private readonly IFlightService _flightService = flightService;
    private readonly IBookingService _bookingService = bookingService;
    public void Show()
    {
        if (!Authenticate()) return;

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Manager Portal")
                .Color(Color.Gold1));

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Manager Options:[/]")
                    .AddChoices(
                        "Filter All Bookings",
                        "Import Flights from CSV",
                        "View Validation Rules",
                        "Logout"
                    ));

            switch (choice)
            {
                case "Filter All Bookings":   FilterBookings(); break;
                case "Import Flights from CSV": ImportFlights(); break;
                case "View Validation Rules": ViewValidationRules(); break;
                case "Logout":                 return;
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }

    private bool Authenticate()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold yellow]Manager Login[/]\n");

        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("[cyan]Enter manager password:[/]")
                .Secret());

        if (password != "admin123")
        {
            AnsiConsole.MarkupLine("[red]Invalid password.[/]");
            Thread.Sleep(1500);
            return false;
        }

        return true;
    }

    private void FilterBookings()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold underline]Filter Bookings[/]\n");
            AnsiConsole.MarkupLine("[grey](Leave blank to skip any filter)[/]\n");

            var flightNumber = Ask("Flight Number");
            if (flightNumber == "BACK") break;

            var passengerName = Ask("Passenger Name");
            if (passengerName == "BACK") break;

            var departureCountry = Ask("Departure Country");
            if (departureCountry == "BACK") break;

            var destinationCountry = Ask("Destination Country");
            if (destinationCountry == "BACK") break;

            var departureAirport = Ask("Departure Airport");
            if (departureAirport == "BACK") break;

            var arrivalAirport = Ask("Arrival Airport");
            if (arrivalAirport == "BACK") break;

            var departureDateInput = Ask("Departure Date (yyyy-MM-dd)");
            if (departureDateInput == "BACK") break;

            var maxPriceInput = Ask("Max Price (USD)");
            if (maxPriceInput == "BACK") break;

            var filter = new BookingSearchFilter
            {
                FlightNumber       = flightNumber,
                PassengerName      = passengerName,
                DepartureCountry   = departureCountry,
                DestinationCountry = destinationCountry,
                DepartureAirport   = departureAirport,
                ArrivalAirport     = arrivalAirport,
                DepartureDate      = ParseDate(departureDateInput),
                MaxPrice           = ParseDecimal(maxPriceInput),
                Type              = ParseType(AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[cyan]Class filter:[/]")
                        .AddChoices("Any", "Economy", "Business", "FirstClass")))
            };

            var bookings = _bookingService.FilterBookings(filter);
            BookingDisplay.ShowBookingsTable(bookings);

            if (!AnsiConsole.Confirm("Filter again?")) break;
        }
    }

    private void ImportFlights()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold underline]Import Flights from CSV[/]\n");

            var path = AnsiConsole.Ask<string>("[cyan]Enter full path to CSV file (or 'BACK' to go to main menu):[/]");
            if (path.Equals("BACK", StringComparison.OrdinalIgnoreCase)) break;

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("Importing flights...", ctx =>
                {
                    Thread.Sleep(500); // Simulate processing
                });

            var result = _flightService.ImportFromCsv(path);
            ValidationDisplay.ShowImportResult(result);

            if (!AnsiConsole.Confirm("Import another file?")) break;
        }
    }

    private void ViewValidationRules()
    {
        while (true)
        {
            AnsiConsole.Clear();
            var details = _flightService.GetFlightValidationDetails();
            ValidationDisplay.ShowValidationDetails(details);

            if (!AnsiConsole.Confirm("View validation rules again?")) break;
        }
    }

    private string? Ask(string label)
    {
        var prompt = new TextPrompt<string>($"[cyan]{label} (or blank, or 'BACK' to go to main menu):[/]")
            .AllowEmpty()
            .Validate(v =>
            {
                if (v.Equals("BACK", StringComparison.OrdinalIgnoreCase))
                    return ValidationResult.Success();
                return ValidationResult.Success();
            });
        
        var value = AnsiConsole.Prompt(prompt);
        if (value.Equals("BACK", StringComparison.OrdinalIgnoreCase))
            return "BACK";
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private DateTime? ParseDate(string? input) =>
        DateTime.TryParse(input, out var d) ? d : null;

    private decimal? ParseDecimal(string? input) =>
        decimal.TryParse(input, out var d) ? d : null;

    private FlightType? ParseType(string input) =>
        input == "Any" ? null : Enum.TryParse<FlightType>(input, out var c) ? c : null;
}
