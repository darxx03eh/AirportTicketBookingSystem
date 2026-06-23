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
                case "Logout": return;
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
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold underline]Filter Bookings[/]\n");
        AnsiConsole.MarkupLine("[grey](Leave blank to skip any filter)[/]\n");

        var filter = new BookingSearchFilter
        {
            FlightNumber       = Ask("Flight Number"),
            PassengerName      = Ask("Passenger Name"),
            DepartureCountry   = Ask("Departure Country"),
            DestinationCountry = Ask("Destination Country"),
            DepartureAirport   = Ask("Departure Airport"),
            ArrivalAirport     = Ask("Arrival Airport"),
            DepartureDate      = ParseDate(Ask("Departure Date (yyyy-MM-dd)")),
            MaxPrice           = ParseDecimal(Ask("Max Price (USD)")),
            Type              = ParseType(AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan]Class filter:[/]")
                    .AddChoices("Any", "Economy", "Business", "FirstClass")))
        };

        var bookings = _bookingService.FilterBookings(filter);
        BookingDisplay.ShowBookingsTable(bookings);
    }

    private void ImportFlights()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold underline]Import Flights from CSV[/]\n");

        var path = AnsiConsole.Ask<string>("[cyan]Enter full path to CSV file:[/]");

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Importing flights...", ctx =>
            {
                Thread.Sleep(500); // Simulate processing
            });

        var result = _flightService.ImportFromCsv(path);
        ValidationDisplay.ShowImportResult(result);
    }

    private void ViewValidationRules()
    {
        AnsiConsole.Clear();
        var details = _flightService.GetFlightValidationDetails();
        ValidationDisplay.ShowValidationDetails(details);
    }

    private string? Ask(string label)
    {
        var value = AnsiConsole.Ask<string>($"[cyan]{label} (or blank):[/]");
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private DateTime? ParseDate(string? input) =>
        DateTime.TryParse(input, out var d) ? d : null;

    private decimal? ParseDecimal(string? input) =>
        decimal.TryParse(input, out var d) ? d : null;

    private FlightType? ParseType(string input) =>
        input == "Any" ? null : Enum.TryParse<FlightType>(input, out var c) ? c : null;
}
