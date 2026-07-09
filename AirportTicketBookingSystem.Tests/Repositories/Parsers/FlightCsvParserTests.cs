using AirportTicketBookingSystem.Infrastructure.IParsers;
using AirportTicketBookingSystem.Infrastructure.Parsers;
using AirportTicketBookingSystem.Tests.Utils.Helpers;
using FluentAssertions;

namespace AirportTicketBookingSystem.Tests.Repositories.Parsers;

using static FlightCsvParserTestHelpers;

public class FlightCsvParserTests : IDisposable
{
    public const string Header =
        "FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice";

    private readonly string _tempDirectory;
    private readonly IFlightCsvParser _sut = new FlightCsvParser();

    public FlightCsvParserTests()
        => _tempDirectory = Path.Combine(Path.GetTempPath(), $"FlightCsvParserTests_{Guid.NewGuid()}");

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, true);
    }

    [Fact]
    public void Parse_ShouldReturnsSingleErrorAtRowZeroAndNoFlights_WhenFileDoesNotExist()
    {
        var missingPath = Path.Combine(_tempDirectory, "does-not-exist.csv");

        var (flights, errors) = _sut.Parse(missingPath);

        flights.Should().BeEmpty();
        errors.Should().ContainSingle();
        errors[0].RowNumber.Should().Be(0);
        errors[0].Messages.Should().ContainSingle(e => e.Contains("File not found"));
    }

    [Fact]
    public void Parse_ShouldReturnsCsvFileIsEmptyError_WhenFileIsEmpty()
    {
        var path = WriteCsv(_tempDirectory);

        var (flights, errors) = _sut.Parse(path);

        flights.Should().BeEmpty();
        errors.Should().ContainSingle();
        errors[0].RowNumber.Should().Be(0);
        errors[0].Messages.Should().ContainSingle(e => e.Contains("CSV file is empty"));
    }

    [Fact]
    public void Parse_ShouldReturnsNoFlightsAndNoErrors_WhenOnlyHeaderRowNoData()
    {
        var path = WriteCsv(_tempDirectory, Header);

        var (flights, errors) = _sut.Parse(path);

        flights.Should().BeEmpty();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldReturnsOneFlightAtRowTwo_WhenSingleValidRow()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,2026-08-01,199.99");

        var (flights, errors) = _sut.Parse(path);
        var (flight, rowNumber) = flights[0];

        errors.Should().BeEmpty();
        flights.Should().ContainSingle();
        rowNumber.Should().Be(2);
        flight.FlightNumber.Should().Be("AB123");
        flight.DepartureCountry.Should().Be("USA");
        flight.DestinationCountry.Should().Be("UK");
        flight.DepartureAirport.Should().Be("JFK");
        flight.ArrivalAirport.Should().Be("LHR");
        flight.DepartureDate.Should().Be(new DateTime(2026, 8, 1));
        flight.BasePrice.Should().Be(199.99m);
    }

    [Fact]
    public void Parse_ShouldReturnsCorrectSequentialRowNumbers_WhenMultipleValidRows()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,2026-08-01,199.99",
            "CD456,France,Germany,CDG,FRA,2026-09-15,89.50");

        var (flights, errors) = _sut.Parse(path);

        errors.Should().BeEmpty();
        flights.Should().HaveCount(2);
        flights[0].rowNumber.Should().Be(2);
        flights[1].rowNumber.Should().Be(3);
    }

    [Fact]
    public void Parse_ShouldTrimsWhitespaceAroundColumnValues()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "  AB123 , USA , UK , JFK , LHR , 2026-08-01 , 199.99 ");

        var (flights, errors) = _sut.Parse(path);

        flights.Should().ContainSingle();
        flights[0].flight.FlightNumber.Should().Be("AB123");
        flights[0].flight.DepartureCountry.Should().Be("USA");
    }

    [Fact]
    public void Parse_ShouldSkippedWithoutError_WhenBlankLinesBetweenDataRows()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,2026-08-01,199.99",
            "",
            "   ",
            "CD456,France,Germany,CDG,FRA,2026-09-15,89.50");

        var (flights, errors) = _sut.Parse(path);

        errors.Should().BeEmpty();
        flights.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_ShouldReturnsColumnCountError_WhenRowWithTooFewColumns()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR");
        
        var (flights, errors) = _sut.Parse(path);
        
        flights.Should().BeEmpty();
        errors.Should().ContainSingle();
        errors[0].RowNumber.Should().Be(2);
        errors[0].Messages.Should().ContainSingle(e => e.Contains("Expected 7 columns, but found 5"));
    }

    [Fact]
    public void Parse_ShouldReturnsDataFormatError_WhenRowWithInvalidDate()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,not-a-date,199.99");
        
        var (flights, errors) = _sut.Parse(path);
        
        flights.Should().BeEmpty();
        errors.Should().ContainSingle();
        errors[0].RowNumber.Should().Be(2);
        errors[0].Messages.Should().ContainSingle(e => e.Contains("Departure date has an invalid format"));
    }

    [Fact]
    public void Parse_ShouldReturnsPriceFormatError_WhenRowWithInvalidPrice()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,2026-08-01,not-a-price");
        
        var (flights, errors) = _sut.Parse(path);
        
        flights.Should().BeEmpty();
        errors.Should().ContainSingle();
        errors[0].RowNumber.Should().Be(2);
        errors[0].Messages.Should().ContainSingle(e => e.Contains("Base price has an invalid format"));
    }

    [Fact]
    public void Parse_ShouldReturnsBothErrorMessagesOnSameError_WhenRowWithBothInvalidDateAndPrice()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,not-a-date,not-a-price");
        
        var (flights, errors) = _sut.Parse(path);

        flights.Should().BeEmpty();
        errors.Should().ContainSingle();
        errors[0].RowNumber.Should().Be(2);
        errors[0].Messages.Should().HaveCount(2);
        errors[0].Messages.Should().Contain(e => e.Contains("Departure date"));
        errors[0].Messages.Should().Contain(e => e.Contains("Base price"));
    }

    [Fact]
    public void Parse_MixOfValidAndInvalidRows_PartitionsIntoFlightsAndErrorsCorrectly()
    {
        var path = WriteCsv(
            _tempDirectory, Header,
            "AB123,USA,UK,JFK,LHR,2026-08-01,199.99",
            "BAD,ROW,TOO,FEW",
            "CD456,France,Germany,CDG,FRA,2026-09-15,89.50",
            "EF789,USA,UK,JFK,LHR,bad-date,50.00");
        
        var (flights, errors) = _sut.Parse(path);

        flights.Should().HaveCount(2);
        flights.Select(flight => flight.flight.FlightNumber)
            .Should().BeEquivalentTo(["AB123", "CD456"]);
        errors.Should().HaveCount(2);
        errors.Select(error => error.RowNumber)
            .Should().BeEquivalentTo([3, 5]);
    }

    [Fact]
    public void Parse_WrongOrHeaderMismatchedColumns_StillParsesPositionallyWithoutValidatingHeader()
    {
        var path = WriteCsv(
            _tempDirectory, "TotallyWrongHeader,Col2,Col3,Col4,Col5,Col6,Col7",
            "AB123,USA,UK,JFK,LHR,2026-08-01,199.99");
        
        var (flights, errors) = _sut.Parse(path);

        flights.Should().ContainSingle();
        errors.Should().BeEmpty();
    }
}