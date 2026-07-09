using System.Text.Json;
using AirportTicketBookingSystem.Infrastructure.Repositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Tests.Utils.Helpers;
using FluentAssertions;

namespace AirportTicketBookingSystem.Tests.Repositories;
using static BookingServiceTestHelpers;

public class FileBookingRepositoryTests : IDisposable
{
    private readonly string _dataDirectory;
    public FileBookingRepositoryTests()
        => _dataDirectory = Path.Combine(Path.GetTempPath(), $"BookingRepositoryTests_{Guid.NewGuid()}");
    public void Dispose()
    {
        if(Directory.Exists(_dataDirectory))
            Directory.Delete(_dataDirectory, true);
    }

    [Fact]
    public void Constructor_CreatesDirectoryAndEmptyFile_WhenNoExistingFile()
    {
        var repo = new FileBookingRepository(_dataDirectory);

        var filePath = Path.Combine(_dataDirectory, "bookings.json");
        
        File.Exists(filePath).Should().BeTrue();
        repo.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldLoadIntoCache_WhenExistingFileWithData()
    {
        Directory.CreateDirectory(_dataDirectory);
        var seed = new List<Booking>
        {
            MakeBooking("B1"), MakeBooking("B2", "F2", "P2")
        };
        File.WriteAllText(
            Path.Combine(_dataDirectory, "bookings.json"),
            JsonSerializer.Serialize(seed));

        var repo = new FileBookingRepository(_dataDirectory);
        
        repo.GetAll().Count().Should().Be(2);
        repo.GetById("B1").Should().NotBeNull();
        repo.GetById("B2").Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldLoadEmptyCacheWithoutError_WhenExistingEmptyArrayFile()
    {
        Directory.CreateDirectory(_dataDirectory);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "bookings.json"), "[]");
        
        var repo = new FileBookingRepository(_dataDirectory);

        repo.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Add_ShouldWritesToFileImmediately()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking());
        
        var json = File.ReadAllText(Path.Combine(_dataDirectory, "bookings.json"));
        var persisted =  JsonSerializer.Deserialize<List<Booking>>(json);

        persisted.Should().NotBeNull();
        persisted.Should().ContainSingle();
        persisted[0]!.Id.Should().Be("B1");
    }

    [Fact]
    public void Add_NewBooking_IsRetrievableFromSameInstance()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        var booking = MakeBooking();
        
        repo.Add(booking);
        
        var book = repo.GetById("B1");
        book.Should().NotBeNull();
        book.Should().BeSameAs(booking);
        repo.GetAll().Should().ContainSingle();
    }

    [Fact]
    public void Add_ThenNewRepositoryInstanceOnSameDirectory_SeesThePersistedBooking()
    {
        var repo1 = new FileBookingRepository(_dataDirectory);
        repo1.Add(MakeBooking());
        
        var repo2 = new FileBookingRepository(_dataDirectory);
        
        repo2.GetById("B1").Should().NotBeNull();
    }

    [Fact]
    public void Add_ShouldNotThrowOnSerialize_WhenBookingWithNavigationPropertiesPopulated()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        var booking = MakeBooking();
        booking.Flight = FlightServiceTestHelpers.MakeFlight();
        booking.Passenger =  PassengerServiceTestHelpers.MakePassenger();
        
        var exception = Record.Exception(() => repo.Add(booking));
        
        exception.Should().BeNull();
    }

    [Fact]
    public void GetById_ShouldReturnsNull_WhenNotFound()
    {
        var repo = new FileBookingRepository(_dataDirectory);

        repo.GetById("B2").Should().BeNull();
    }

    [Fact]
    public void GetByPassengerId_ShouldReturnsOnlyMatchingBookings()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking("B1", passengerId: "P1"));
        repo.Add(MakeBooking("B2", passengerId: "P2"));
        repo.Add(MakeBooking("B3", passengerId: "P1"));

        var result = repo.GetByPassengerId("P1").ToList();

        result.Count.Should().Be(2);
        result.All(b => Equals(b.PassengerId, "P1")).Should().BeTrue();
    }

    [Fact]
    public void GetByPassengerId_ShouldReturnsEmpty_WhenNoMatches()
    {
        var repo  = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking("B1",  passengerId: "P1"));
        
        var result = repo.GetByPassengerId("P2").ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Update_ShouldReplacesItAndPersists_WhenExistingBooking()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking());
        
        var updated =  MakeBooking("B1", "F2", "P1");
        updated.Type = FlightType.Business;
        repo.Update(updated);

        var result = repo.GetById("B1");
        result!.FlightId.Should().Be("F2");
        result.Type.Should().Be(FlightType.Business);
        
        var repo2 = new FileBookingRepository(_dataDirectory);
        repo2.GetById("B1")!.FlightId.Should().Be("F2");
    }

    [Fact]
    public void Update_ShouldNotAddItAndDoesNotThrow_WhenNonExistenId()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking());
 
        var exception = Record.Exception(() => repo.Update(MakeBooking("DOES-NOT-EXIST")));
 
        exception.Should().BeNull();
        repo.GetAll().Should().ContainSingle();
        repo.GetById("B2").Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldRemovesItAndPersists_WhenExistingBooking()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking());
        repo.Add(MakeBooking("B2"));
 
        repo.Delete("B1");
 
        repo.GetById("B1").Should().BeNull();
        repo.GetAll().Should().ContainSingle();
 
        var repo2 = new FileBookingRepository(_dataDirectory);
        repo2.GetById("B1").Should().BeNull();
    }
    
    [Fact]
    public void Delete_NonExistentId_ShouldNoOpAndDoesNotThrow()
    {
        var repo = new FileBookingRepository(_dataDirectory);
        repo.Add(MakeBooking());
 
        var exception = Record.Exception(() => repo.Delete("B2"));
 
        exception.Should().BeNull();
        repo.GetAll().Should().ContainSingle();
    }
}