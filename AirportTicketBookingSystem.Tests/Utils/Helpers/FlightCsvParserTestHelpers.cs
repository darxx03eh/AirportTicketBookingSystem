namespace AirportTicketBookingSystem.Tests.Utils.Helpers;

public static class FlightCsvParserTestHelpers
{
    public static string WriteCsv(string tempDirectory, params string[] lines)
    {
        Directory.CreateDirectory(tempDirectory);
        var path = Path.Combine(tempDirectory, $"{Guid.NewGuid()}.csv");
        File.WriteAllLines(path, lines);
        return path;
    }
}