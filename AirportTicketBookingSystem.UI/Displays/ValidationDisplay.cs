using AirportTicketBookingSystem.Service.DTOs;
using Spectre.Console;

namespace AirportTicketBookingSystem.UI.Displays;

public class ValidationDisplay
{
    public static void ShowValidationDetails(IEnumerable<ValidationFieldInfo> details)
    {
        AnsiConsole.MarkupLine("\n[bold underline]Flight Model Validation Details[/]\n");

        foreach (var field in details)
        {
            var panel = new Panel(
                $"[bold]Type:[/]        {field.Type}\n" +
                $"[bold]Constraints:[/] [yellow]{Markup.Escape(field.Constraints)}[/]")
            {
                Header = new PanelHeader($"[cyan]{field.FieldName}[/]"),
                Border = BoxBorder.Rounded
            };

            AnsiConsole.Write(panel);
        }
    }

    public static void ShowImportResult(ImportResult result)
    {
        AnsiConsole.MarkupLine($"\n[bold]Import Summary[/]");
        AnsiConsole.MarkupLine($"  Total rows processed: [cyan]{result.TotalRows}[/]");
        AnsiConsole.MarkupLine($"  Successfully imported: [green]{result.SuccessfulFlights.Count}[/]");
        AnsiConsole.MarkupLine($"  Failed rows:          [red]{result.Errors.Count}[/]");

        if (result.HasErrors)
        {
            AnsiConsole.MarkupLine("\n[bold red]Validation Errors:[/]");

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Spectre.Console.Color.Red)
                .AddColumn("[bold]Row[/]")
                .AddColumn("[bold]Error(s)[/]");

            foreach (var error in result.Errors.OrderBy(e => e.RowNumber))
            {
                table.AddRow(
                    $"[red]{error.RowNumber}[/]",
                    string.Join("\n", error.Messages.Select(m => $"• {m}"))
                );
            }

            AnsiConsole.Write(table);
        }
    }
}
