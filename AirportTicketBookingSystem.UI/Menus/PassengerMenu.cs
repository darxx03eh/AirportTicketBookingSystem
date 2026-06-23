using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Service.DTOs;
using AirportTicketBookingSystem.Service.Interfaces;
using AirportTicketBookingSystem.UI.Displays;
using AirportTicketBookingSystem.UI.IMenus;
using Spectre.Console;

namespace AirportTicketBookingSystem.UI.Menus;

public class PassengerMenu(
    IFlightService flightService,
    IBookingService bookingService,
    IPassengerService passengerService
    ) : IPassengerMenu
{
    private readonly IFlightService _flightService = flightService;
    private readonly IBookingService _bookingService = bookingService;
    private readonly IPassengerService _passengerService = passengerService;
    private Passenger? _currentPassenger;
    public void Show()
    {
        _currentPassenger = LoginOrRegister();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Passenger Portal")
                .Color(Color.SteelBlue1));
            AnsiConsole.MarkupLine($"[grey]Logged in as:[/] [cyan]{_currentPassenger.FullName}[/]\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]What would you like to do?[/]")
                    .AddChoices(
                        "Search & Book a Flight",
                        "View My Bookings",
                        "Cancel a Booking",
                        "Modify a Booking",
                        "Logout"
                    ));

            switch (choice)
            {
                case "Search & Book a Flight": SearchAndBook(); break;
                case "View My Bookings":       ViewMyBookings(); break;
                case "Cancel a Booking":       CancelBooking(); break;
                case "Modify a Booking":       ModifyBooking(); break;
                case "Logout":                 return;
            }

            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }

    private Passenger LoginOrRegister()
    {
        AnsiConsole.MarkupLine("[bold]Enter your details to login or register:[/]\n");

        var firstName = AnsiConsole.Ask<string>("[cyan]First Name:[/]");
        var lastName  = AnsiConsole.Ask<string>("[cyan]Last Name:[/]");
        var email     = AnsiConsole.Ask<string>("[cyan]Email:[/]");

        return _passengerService.GetOrCreate(firstName, lastName, email);
    }

    private void SearchAndBook()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold underline]Search Flights[/]\n");

        var filter  = BuildFlightFilter();
        var flights = _flightService.SearchFlights(filter).ToList();

        FlightDisplay.ShowFlightsTable(flights);

        if (!flights.Any()) return;

        var book = AnsiConsole.Confirm("\nWould you like to book one of these flights?");
        if (!book) return;

        var flightIndex = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter flight [cyan]#[/] to book:")
                .Validate(n => n >= 1 && n <= flights.Count
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"Enter a number between 1 and {flights.Count}.")));

        var selectedFlight = flights[flightIndex - 1];

        var flightClass = AnsiConsole.Prompt(
            new SelectionPrompt<FlightType>()
                .Title("Select [bold]class[/]:")
                .AddChoices(FlightType.Economy, FlightType.Business, FlightType.FirstClass));

        AnsiConsole.MarkupLine($"\n[grey]Price: [green]${selectedFlight.GetPriceForType(flightClass):F2}[/][/]");

        if (!AnsiConsole.Confirm("Confirm booking?")) return;

        try
        {
            var booking = _bookingService.CreateBooking(_currentPassenger!.Id, selectedFlight.Id, flightClass);
            BookingDisplay.ShowBookingConfirmation(booking);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }

    private void ViewMyBookings()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold underline]My Bookings[/]\n");

        var bookings = _bookingService.GetPassengerBookings(_currentPassenger!.Id);
        BookingDisplay.ShowBookingsTable(bookings);
    }

    private void CancelBooking()
    {
        AnsiConsole.Clear();
        var bookings = _bookingService.GetPassengerBookings(_currentPassenger!.Id).ToList();

        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You have no bookings to cancel.[/]");
            return;
        }

        BookingDisplay.ShowBookingsTable(bookings);

        var index = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter booking [cyan]#[/] to cancel:")
                .Validate(n => n >= 1 && n <= bookings.Count
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Invalid selection.")));

        var booking = bookings[index - 1];

        if (!AnsiConsole.Confirm($"[red]Cancel booking for flight {booking.Flight?.FlightNumber}?[/]")) return;

        try
        {
            _bookingService.CancelBooking(booking.Id, _currentPassenger!.Id);
            AnsiConsole.MarkupLine("[green]Booking cancelled successfully.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }

    private void ModifyBooking()
    {
        AnsiConsole.Clear();
        var bookings = _bookingService.GetPassengerBookings(_currentPassenger!.Id).ToList();

        if (!bookings.Any())
        {
            AnsiConsole.MarkupLine("[yellow]You have no bookings to modify.[/]");
            return;
        }

        BookingDisplay.ShowBookingsTable(bookings);

        var index = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter booking [cyan]#[/] to modify:")
                .Validate(n => n >= 1 && n <= bookings.Count
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Invalid selection.")));

        var booking = bookings[index - 1];

        FlightType? newType = null;
        if (AnsiConsole.Confirm("Change travel class?"))
        {
            newType = AnsiConsole.Prompt(
                new SelectionPrompt<FlightType>()
                    .Title("Select new [bold]class[/]:")
                    .AddChoices(FlightType.Economy, FlightType.Business, FlightType.FirstClass));
        }

        try
        {
            var updated = _bookingService.ModifyBooking(booking.Id, _currentPassenger!.Id, null, newType);
            AnsiConsole.MarkupLine("[green]Booking updated successfully.[/]");
            BookingDisplay.ShowBookingConfirmation(updated);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }

    private FlightSearchFilter BuildFlightFilter()
    {
        AnsiConsole.MarkupLine("[grey](Leave blank to skip any filter)[/]\n");

        var departureCountry   = Ask("Departure Country");
        var destinationCountry = Ask("Destination Country");
        var departureAirport   = Ask("Departure Airport");
        var arrivalAirport     = Ask("Arrival Airport");
        var dateInput          = Ask("Departure Date yyyy-MM-dd");
        var priceInput         = Ask("Max Price in USD");

        var classChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Preferred Class:[/]")
                .AddChoices("Any", "Economy", "Business", "FirstClass"));

        FlightType? selectedClass = classChoice == "Any"
            ? null
            : Enum.Parse<FlightType>(classChoice);

        return new FlightSearchFilter
        {
            DepartureCountry   = departureCountry,
            DestinationCountry = destinationCountry,
            DepartureAirport   = departureAirport,
            ArrivalAirport     = arrivalAirport,
            DepartureDate      = ParseOptionalDate(dateInput),
            MaxPrice           = ParseOptionalDecimal(priceInput),
            Type              = selectedClass
        };
    }

    private string? Ask(string label)
    {
        var value = AnsiConsole.Ask<string>($"[cyan]{label} (or blank):[/]");
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private DateTime? ParseOptionalDate(string? input) =>
        DateTime.TryParse(input, out var d) ? d : null;

    private decimal? ParseOptionalDecimal(string? input) =>
        decimal.TryParse(input, out var d) ? d : null;
}