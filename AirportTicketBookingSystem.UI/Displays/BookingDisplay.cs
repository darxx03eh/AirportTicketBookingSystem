using AirportTicketBookingSystem.Models.Entities;
using Spectre.Console;

namespace AirportTicketBookingSystem.UI.Displays;

public class BookingDisplay
{
    public static void ShowBookingsTable(IEnumerable<Booking> bookings)
    {
        var bookingList = bookings.ToList();

        if (!bookingList.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No bookings found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Purple)
            .AddColumn(new TableColumn("[bold]#[/]").Centered())
            .AddColumn("[bold]Booking ID[/]")
            .AddColumn("[bold]Passenger[/]")
            .AddColumn("[bold]Flight[/]")
            .AddColumn("[bold]Route[/]")
            .AddColumn("[bold]Date[/]")
            .AddColumn("[bold]Class[/]")
            .AddColumn("[bold]Price[/]")
            .AddColumn("[bold]Booked At[/]");

        for (int i = 0; i < bookingList.Count; i++)
        {
            var b = bookingList[i];
            var route = b.Flight != null
                ? $"{b.Flight.DepartureCountry} → {b.Flight.DestinationCountry}"
                : "N/A";

            table.AddRow(
                $"[grey]{i + 1}[/]",
                $"[dim]{b.Id[..8]}...[/]",
                b.Passenger?.FullName ?? "Unknown",
                $"[cyan]{b.Flight?.FlightNumber ?? "N/A"}[/]",
                route,
                $"[blue]{b.Flight?.DepartureDate:yyyy-MM-dd}[/]",
                $"[magenta]{b.Type}[/]",
                $"[green]${b.TotalPrice:F2}[/]",
                $"[grey]{b.BookedAt:yyyy-MM-dd}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[grey]Total: {bookingList.Count} booking(s).[/]");
    }

    public static void ShowBookingConfirmation(Booking booking)
    {
        var panel = new Panel(
            $"""
             [bold green]✓ Booking Confirmed![/]

             [bold]Booking ID:[/]  {booking.Id}
             [bold]Passenger:[/]   {booking.Passenger?.FullName}
             [bold]Flight:[/]      {booking.Flight?.FlightNumber}
             [bold]Route:[/]       {booking.Flight?.DepartureCountry} → {booking.Flight?.DestinationCountry}
             [bold]Departure:[/]   {booking.Flight?.DepartureDate:yyyy-MM-dd HH:mm}
             [bold]Class:[/]       {booking.Type}
             [bold]Total Price:[/] [green]${booking.TotalPrice:F2}[/]
             """)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        };

        AnsiConsole.Write(panel);
    }
}
