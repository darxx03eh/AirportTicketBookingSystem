using System.Text.Json;
using AirportTicketBookingSystem.Infrastructure.Repositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Tests.Utils.Helpers;
using FluentAssertions;

namespace AirportTicketBookingSystem.Tests.Repositories;
using static FlightServiceTestHelpers;
public class FileFlightRepositoryTests : IDisposable
{
    private readonly string _dataDirectory;
    public FileFlightRepositoryTests() 
        => _dataDirectory = Path.Combine(Path.GetTempPath(), $"FlightRepositoryTests_{Guid.NewGuid()}");
    public void Dispose()
    {
        if(Directory.Exists(_dataDirectory))
            Directory.Delete(_dataDirectory, true);
    }

    [Fact]
    public void Constructor_ShouldCreatesDirectoryAndEmptyFile_WhenThereIsNoExistingFile()
    {
        var repo = new FileFlightRepository(_dataDirectory);

        var filePath = Path.Combine(_dataDirectory, "flights.json");
        
        File.Exists(filePath).Should().BeTrue();
        repo.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldLoadsIntoCache_WhenThereIsExistingFileWithData()
    {
        Directory.CreateDirectory(_dataDirectory);
        var seed = new List<Flight>
        {
            MakeFlight("F1"),
            MakeFlight("F2", "CD456")
        };
        
        File.WriteAllText(
            Path.Combine(_dataDirectory, "flights.json"),
            JsonSerializer.Serialize(seed));

        var repo = new FileFlightRepository(_dataDirectory);

        repo.GetAll().Should().HaveCount(2);
        repo.GetById("F1").Should().NotBeNull();
        repo.GetById("F2").Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldLoadsEmptyCacheWithoutError_WhenExistingEmptyArrayFile()
    {
        Directory.CreateDirectory(_dataDirectory);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "flights.json"), string.Empty);

        var repo = new FileFlightRepository(_dataDirectory);

        repo.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Add_NewFlight_IsRetrievableFromSameInstance()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        var flight = MakeFlight();
        
        repo.Add(flight);

        repo.GetById("F1").Should().BeSameAs(flight);
        repo.GetAll().Should().ContainSingle();
    }

    [Fact]
    public void Add_ShouldWritesToFileImmediately()
    {
        var repo =  new FileFlightRepository(_dataDirectory);
        repo.Add(MakeFlight());
        
        var json = File.ReadAllText(Path.Combine(_dataDirectory, "flights.json"));
        var persisted = JsonSerializer.Deserialize<List<Flight>>(json);

        persisted.Should().NotBeNull();
        persisted.Should().Contain(flight => Equals(flight.Id, "F1"));
    }

    [Fact]
    public void Add_ThenNewRepositoryInstanceOnSameDirectory_SeesThePersistedFlight()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        repo.Add(MakeFlight());

        var repo2 = new FileFlightRepository(_dataDirectory);

        repo2.GetById("F1").Should().NotBeNull();
    }

    [Fact]
    public void AddRange_MultipleFlights_AllRetrievableAndPersisted()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        var flights = new List<Flight>()
        {
            MakeFlight("F1"),
            MakeFlight("F2", "CD456"),
            MakeFlight("F3", "EF789")
        };
        
        repo.AddRange(flights);

        var getFlights = repo.GetAll().ToList();
        getFlights.Should().HaveCount(3);
        getFlights.Should().Contain(flights);
    }

    [Fact]
    public void AddRange_AppendsToExistingFlightsRatherThanReplacing()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        repo.Add(MakeFlight());
        
        repo.AddRange(new[]
        {
            MakeFlight("F2", "CD456"),
            MakeFlight("F3", "EF789")
        });

        repo.GetAll().Should().HaveCount(3);
        repo.GetById("F1").Should().NotBeNull();
    }

    [Fact]
    public void GetById_ShouldReturnsNull_IfFlightNotFound()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        
        repo.GetById("F1").Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldRemoveItAndPersists_WhenThereExistingFlight()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        var repo2 = new FileFlightRepository(_dataDirectory);
        
        repo.Add(MakeFlight());
        repo.Add(MakeFlight("F2", "CD456"));
        
        repo.Delete("F1");
        
        repo.GetById("F1").Should().BeNull();
        repo.GetAll().Should().ContainSingle();
        
        
        repo2.GetById("F1").Should().BeNull();
    }

    [Fact]
    public void Delete_NonExistenId_IsNoOpAndDoesNotThrow()
    {
        var repo = new FileFlightRepository(_dataDirectory);
        repo.Add(MakeFlight());
        
        Action act = () => repo.Delete("F2");

        act.Should().NotThrow();
        repo.GetAll().Should().ContainSingle();
    }
}