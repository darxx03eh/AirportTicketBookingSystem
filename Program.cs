using AirportTicketBookingSystem.Infrastructure.IParsers;
using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Infrastructure.Parsers;
using AirportTicketBookingSystem.Infrastructure.Repositories;
using AirportTicketBookingSystem.Service.Implementations;
using AirportTicketBookingSystem.Service.Interfaces;
using AirportTicketBookingSystem.UI.IMenus;
using AirportTicketBookingSystem.UI.Menus;
using Spectre.Console;

namespace AirportTicketBookingSystem;

class Program
{
    static void Main(string[] args)
    {
        var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "AirportTicketBookingSystem.Infrastructure", "Data");
        IFlightCsvParser csvParser = new FlightCsvParser();

        IFlightRepository flightRepository = new FileFlightRepository(dataDirectory);
        IBookingRepository bookingRepository = new FileBookingRepository(dataDirectory);
        IPassengerRepository passengerRepository = new FilePassengerRepository(dataDirectory);
        
        IFlightService flightService  = new FlightService(flightRepository, csvParser);
        IBookingService bookingService = new BookingService(bookingRepository, flightRepository, passengerRepository);
        IPassengerService passengerService = new PassengerService(passengerRepository);

        IPassengerMenu passengerMenu = new PassengerMenu(flightService, bookingService, passengerService);
        IManagerMenu managerMenu   = new ManagerMenu(flightService, bookingService);
        while (true)
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new FigletText("SkyBooker")
                    .Centered()
                    .Color(Color.DeepSkyBlue1));

            AnsiConsole.Write(
                new Rule("[grey]Airport Ticket Booking System[/]")
                    .RuleStyle(Style.Parse("grey")));

            AnsiConsole.WriteLine();

            var role = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Please select your role:[/]")
                    .AddChoices("Passenger", "Manager", "Exit"));

            switch (role)
            {
                case "Passenger": passengerMenu.Show(); break;
                case "Manager":   managerMenu.Show();   break;
                case "Exit":
                    AnsiConsole.MarkupLine("\n[grey]Thank you for using SkyBooker. Goodbye![/]");
                    return;
            }
        }
    }
}