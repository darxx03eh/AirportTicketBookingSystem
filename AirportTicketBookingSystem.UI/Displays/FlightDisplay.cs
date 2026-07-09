using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using Spectre.Console;

namespace AirportTicketBookingSystem.UI.Displays;

public class FlightDisplay
{
    public static void ShowFlightsTable(IEnumerable<Flight> flights, FlightType? selectedType = null)
    {
        var flightList = flights.ToList();

        if (!flightList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No flights found matching your criteria.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.SteelBlue1)
            .AddColumn(new TableColumn("[bold]#[/]").Centered())
            .AddColumn("[bold]Flight[/]")
            .AddColumn("[bold]From[/]")
            .AddColumn("[bold]To[/]")
            .AddColumn("[bold]Departure Airport[/]")
            .AddColumn("[bold]Arrival Airport[/]")
            .AddColumn("[bold]Date[/]")
            .AddColumn("[bold]Economy[/]")
            .AddColumn("[bold]Business[/]")
            .AddColumn("[bold]First Class[/]");

        for (int i = 0; i < flightList.Count; i++)
        {
            var f = flightList[i];
            table.AddRow(
                $"[grey]{i + 1}[/]",
                $"[cyan]{f.FlightNumber}[/]",
                f.DepartureCountry,
                f.DestinationCountry,
                f.DepartureAirport,
                f.ArrivalAirport,
                $"[blue]{f.DepartureDate:yyyy-MM-dd HH:mm}[/]",
                $"[green]${f.GetPriceForType(FlightType.Economy):F2}[/]",
                $"[yellow]${f.GetPriceForType(FlightType.Business):F2}[/]",
                $"[gold1]${f.GetPriceForType(FlightType.FirstClass):F2}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[grey]Total: {flightList.Count} flight(s) found.[/]");
    }
}
