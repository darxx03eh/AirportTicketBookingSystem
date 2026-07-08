using AirportTicketBookingSystem.Infrastructure.IParsers;
using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.DTOs;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Service.DTOs;
using AirportTicketBookingSystem.Service.Implementations;
using AirportTicketBookingSystem.Service.Interfaces;
using FluentAssertions;
using Moq;
using static AirportTicketBookingSystem.Tests.Utils.Helpers.FlightServiceTestHelpers;

namespace AirportTicketBookingSystem.Tests.Services;

public class FlightServiceTests
{
    private readonly Mock<IFlightRepository> _flightRepository = new();
    private readonly Mock<IFlightCsvParser> _flightCsvParser = new();
    private readonly IFlightService _sut;
    public FlightServiceTests() => _sut = new FlightService(_flightRepository.Object, _flightCsvParser.Object);

    [Fact]
    public void SearchFlights_ShouldReturnsAllOrderedByDepartureDate_WhenNoFilters()
    {
        var late = MakeFlight("F1", departureDate: new DateTime(2026, 9, 1));
        var early = MakeFlight("F2", departureDate: new DateTime(2026, 7, 1));
        _flightRepository.Setup(f => f.GetAll())
            .Returns([late, early]);

        var result = _sut.SearchFlights(new FlightSearchFilter())
            .ToList();

        result.Should().Contain([late, early]);
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("F2");
        result[0].Should().Be(early);
        result[1].Id.Should().Be("F1");
        result[1].Should().Be(late);
    }

    [Fact]
    public void SearchFlights_ByDepartureCountry_IsCaseInsensitiveSubstringMatch()
    {
        var match = MakeFlight("F1", departureCountry: "United States");
        var noMatch = MakeFlight("F2", departureCountry: "France");
        _flightRepository.Setup(f => f.GetAll())
            .Returns([match, noMatch]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            DepartureCountry = "united"
        }).ToList();

        result.Should().ContainSingle();
        result.First().Id.Should().Be("F1");
    }

    [Fact]
    public void SearchFlights_ShouldFiltersCorrectly_ByDestinationCountry()
    {
        var match = MakeFlight("F1", destinationCountry: "Germany");
        var noMatch = MakeFlight("F2", destinationCountry: "Spain");
        _flightRepository.Setup(f => f.GetAll())
            .Returns([match, noMatch]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            DestinationCountry = "germany"
        }).ToList();
        
        result.Should().ContainSingle();
        result.First().Id.Should().Be("F1");
    }

    [Fact]
    public void SearchFlights_ShouldFiltersCorrectly_ByDepartureAirport()
    {
        var match = MakeFlight("F2", departureAirport: "JFK");
        var noMatch = MakeFlight("F1", departureAirport: "LAX");
        _flightRepository.Setup(f => f.GetAll())
            .Returns([match, noMatch]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            DepartureAirport = "jfk"
        }).ToList();
        
        result.Should().ContainSingle();
        result.First().Id.Should().Be("F2");
    }

    [Fact]
    public void SearchFlights_ShouldFiltersCorrectly_ByArrivalAirport()
    {
        var match = MakeFlight("F2", arrivalAirport: "LHR");
        var noMatch = MakeFlight("F1", arrivalAirport: "CDG");
        _flightRepository.Setup(f => f.GetAll())
            .Returns([match, noMatch]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            ArrivalAirport = "lhr"
        }).ToList();
        
        result.Should().ContainSingle();
        result.First().Id.Should().Be("F2");
    }

    [Fact]
    public void SearchFlights_ShouldMatchesDateOnlyIgnoringTime_ByDepartureDate()
    {
        var match = MakeFlight("F1", departureDate: new DateTime(2026, 8, 1, 14, 30, 0));
        var noMatch = MakeFlight("F2", departureDate: new DateTime(2026, 8, 2));
        _flightRepository.Setup(f => f.GetAll())
            .Returns([match, noMatch]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            DepartureDate = new DateTime(2026, 8, 1)
        }).ToList();
        
        result.Should().ContainSingle();
        result.First().Id.Should().Be("F1");
    }

    [Fact]
    public void SearchFlights_IsIgnored_MaxPriceWithoutType()
    {
        var flight = MakeFlight();
        _flightRepository.Setup(f => f.GetAll())
            .Returns([flight]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            MaxPrice = 1
        }).ToList();

        result.Should().ContainSingle();
    }

    [Fact]
    public void SearchFlights_ShouldFilterUsingGetPriceForType_MaxPriceWithType()
    {
        var cheap = MakeFlight("F1");
        var expensive = MakeFlight("F2");
        _flightRepository.Setup(f => f.GetAll())
            .Returns([cheap, expensive]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            MaxPrice = cheap.GetPriceForType(FlightType.Economy),
            Type = FlightType.Economy
        }).ToList();
        
        result.Should().Contain(cheap);
    }

    [Fact]
    public void SearchFlights_ShouldCombinesMultipleFiltersWithAnd()
    {
        var match = MakeFlight("F1", departureCountry: "USA", arrivalAirport: "LHR");
        var wrongCountry = MakeFlight("F2", departureCountry: "France", arrivalAirport: "LHR");
        var wrongAirport = MakeFlight("F3", departureCountry: "USA", arrivalAirport: "CDG");
        _flightRepository.Setup(f => f.GetAll())
            .Returns([match, wrongCountry, wrongAirport]);

        var result = _sut.SearchFlights(new FlightSearchFilter()
        {
            DepartureCountry = "usa",
            ArrivalAirport = "lhr"
        }).ToList();
        
        result.Should().ContainSingle();
        result.Should().Contain(match);
        result.First().Id.Should().Be("F1");
    }

    [Fact]
    public void GetById_ShouldReturnsFlight_IfFound()
    {
        var flight = MakeFlight();
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns(flight);
        
        var result = _sut.GetById("F1");
        
        result.Should().BeSameAs(flight);
    }
    
    [Fact]
    public void GetById_ShouldReturnsNull_IfNotFound()
    {
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns((Flight?)null);
        
        var result =  _sut.GetById("F1");
        
        result.Should().BeNull();
    }

    [Fact]
    public void ImportFromCsv_AddsAllAndReturnsNoErrors_WhenAllIsValidRows()
    {
        var flight1 = MakeParsedFlight("AB123");
        var flight2 = MakeParsedFlight("CD456");
        _flightCsvParser.Setup(p => p.Parse("flights.csv")).Returns((
            Flights: new List<(Flight flight, int rowNumber)> { (flight1, 2), (flight2, 3) },
            ParseErrors: new List<ImportError>()
        ));
        
        var result = _sut.ImportFromCsv("flights.csv");
        
        result.SuccessfulFlights.Count.Should().Be(2);
        result.Errors.Should().BeEmpty();
        result.HasErrors.Should().BeFalse();
        result.TotalRows.Should().Be(2);
        _flightRepository.Verify(r => r.AddRange(It.Is<List<Flight>>(l => l.Count == 2)), Times.Once);
    }

    [Fact]
    public void ImportFromCsv_RowFailingDataAnnotationsValidations_GoesToErrorsNotSuccess()
    {
        var invalidFlight = MakeParsedFlight(number: "", departureCountry: "");
        _flightCsvParser.Setup(p => p.Parse("flights.csv")).Returns((
            Flights: new List<(Flight flight, int rowNumber)> { (invalidFlight, 5) },
            ParseErrors: new List<ImportError>()
        ));
        
        var result = _sut.ImportFromCsv("flights.csv");

        result.SuccessfulFlights.Should().BeEmpty();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RowNumber.Should().Be(5);
        result.Errors[0].Messages.Should().NotBeEmpty();
        
        _flightRepository.Verify(f => f.AddRange(It.IsAny<List<Flight>>()), Times.Never);
    }
    
    [Fact]
    public void ImportFromCsv_ParserLevelErrors_ArePassedThroughToResult()
    {
        var parserError = new ImportError { RowNumber = 3, Messages = 
            { "Departure date has an invalid format. Expected: YYYY-MM-DD." } 
        };
        _flightCsvParser.Setup(p => p.Parse("flights.csv")).Returns((
            Flights: new List<(Flight flight, int rowNumber)>(),
            ParseErrors: new List<ImportError> { parserError }
        ));
 
        var result = _sut.ImportFromCsv("flights.csv");

        result.SuccessfulFlights.Should().BeEmpty();
        result.Errors.Should().Contain(error => error.RowNumber == 3);
        _flightRepository.Verify(r => r.AddRange(It.IsAny<List<Flight>>()), Times.Never);
    }
    
    [Fact]
    public void ImportFromCsv_MixOfValidAndInvalidRows_PartitionsCorrectly()
    {
        var valid = MakeParsedFlight("AB123");
        var invalid = MakeParsedFlight(number: "");
        _flightCsvParser.Setup(p => p.Parse("flights.csv")).Returns((
            Flights: new List<(Flight flight, int rowNumber)> { (valid, 1), (invalid, 2) },
            ParseErrors: new List<ImportError>()
        ));
 
        var result = _sut.ImportFromCsv("flights.csv");
        
        result.SuccessfulFlights.Should().ContainSingle();
        result.Errors.Should().ContainSingle();
        _flightRepository.Verify(r => r.AddRange(It.Is<List<Flight>>(
            l => l.Count == 1 && l[0].FlightNumber == "AB123")), Times.Once);
    }
    
    [Fact]
    public void ImportFromCsv_ShouldDoesNotCallAddRange_WhenNoRowsAtAll()
    {
        _flightCsvParser.Setup(p => p.Parse("empty.csv")).Returns((
            Flights: new List<(Flight flight, int rowNumber)>(),
            ParseErrors: new List<ImportError>()
        ));
 
        var result = _sut.ImportFromCsv("empty.csv");
        
        result.SuccessfulFlights.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
        _flightRepository.Verify(r => r.AddRange(It.IsAny<List<Flight>>()), Times.Never);
    }
    
    [Fact]
    public void ImportFromCsv_FileNotFound_ReturnsParseErrorAndDoesNotCallAddRange()
    {
        _flightCsvParser.Setup(p => p.Parse("missing.csv")).Returns((
            Flights: new List<(Flight flight, int rowNumber)>(),
            ParseErrors: new List<ImportError> { new() { RowNumber = 0, Messages = { "File not found: missing.csv" } } }
        ));
 
        var result = _sut.ImportFromCsv("missing.csv");
        
        result.SuccessfulFlights.Should().BeEmpty();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RowNumber.Should().Be(0);
        _flightRepository.Verify(r => r.AddRange(It.IsAny<List<Flight>>()), Times.Never);
    }
}