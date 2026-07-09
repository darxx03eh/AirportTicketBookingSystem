using System.Text.Json;
using AirportTicketBookingSystem.Infrastructure.Repositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Tests.Attributes;
using AirportTicketBookingSystem.Tests.TestModels.Repositories;
using AirportTicketBookingSystem.Tests.Utils.Helpers;
using FluentAssertions;

namespace AirportTicketBookingSystem.Tests.Repositories;

using static PassengerServiceTestHelpers;

public class FilePassengerRepositoryTests : IDisposable
{
    private readonly string _dataDirectory;

    public FilePassengerRepositoryTests()
        => _dataDirectory = Path.Combine(Path.GetTempPath(), $"PassengerRepositoryTest_{Guid.NewGuid()}");

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
            Directory.Delete(_dataDirectory, true);
    }

    [Fact]
    public void Constructor_ShouldCreateDirectoryAndEmptyFile_WhenThereIsNoExistingFile()
    {
        var repo = new FilePassengerRepository(_dataDirectory);

        var filePath = Path.Combine(_dataDirectory, "passengers.json");

        File.Exists(filePath).Should().BeTrue();
        repo.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldLoadIntoCache_WhenThereIsExistingFileWithData()
    {
        Directory.CreateDirectory(_dataDirectory);
        var seed = new List<Passenger>()
        {
            MakePassenger(),
            MakePassenger("P2", "Abdullah", "Noor", "abdullah@example.com")
        };

        File.WriteAllText(
            Path.Combine(_dataDirectory, "passengers.json"),
            JsonSerializer.Serialize(seed));

        var repo = new FilePassengerRepository(_dataDirectory);

        repo.GetAll().Should().HaveCount(2);
        repo.GetById("P1").Should().NotBeNull();
        repo.GetById("P2").Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldLoadEmptyCacheWithoutError_WhenExistingEmptyArrayFile()
    {
        Directory.CreateDirectory(_dataDirectory);
        File.WriteAllText(Path.Combine(_dataDirectory, "passengers.json"), "[]");

        var repo = new FilePassengerRepository(_dataDirectory);

        repo.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Add_NewPassenger_IsRetrievableFromSameInstance()
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        var passenger = MakePassenger();

        repo.Add(passenger);

        repo.GetById("P1").Should().BeSameAs(passenger);
        repo.GetAll().Should().ContainSingle();
    }

    [Fact]
    public void Add_ShouldWriteToFileImmediately()
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        repo.Add(MakePassenger());

        var json = File.ReadAllText(Path.Combine(_dataDirectory, "passengers.json"));
        var persisted = JsonSerializer.Deserialize<List<Passenger>>(json);

        persisted.Should().NotBeNull();
        persisted.Should().Contain(passenger => Equals(passenger.Id, "P1") &&
                                                Equals(passenger.FirstName, "Mahmoud") &&
                                                Equals(passenger.LastName, "Darawsheh") &&
                                                Equals(passenger.Email, "mahmoud@example.com")
        );
    }

    [Fact]
    public void Add_ThenNewRepositoryInstanceOnSameDirectory_SeesThePersistedPassenger()
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        repo.Add(MakePassenger());

        var repo2 = new FilePassengerRepository(_dataDirectory);

        repo2.GetById("P1").Should().NotBeNull();
    }

    [Fact]
    public void GetByEmail_ShouldReturnsPassenger_WithExactCaseMatch()
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        var passenger = MakePassenger(email: "darawsheh@example.com");
        repo.Add(passenger);

        repo.GetByEmail("darawsheh@example.com").Should().NotBeNull();
        repo.GetByEmail("darawsheh@example.com").Should().BeSameAs(passenger);
    }

    [Theory]
    [CsvData(typeof(EmailTestCaseRow), "TestData", "Repositories", "emails-tests-data.csv")]
    public void GetByEmail_IsCaseInsensitive_MatchesRegardlessOfCasing(EmailTestCaseRow testCase)
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        repo.Add(MakePassenger());

        var result = repo.GetByEmail("mahmoud@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("mahmoud@example.com");
    }

    [Fact]
    public void GetByEmail_ShouldReturnsNull_IfThereIsNoMatchingEmail()
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        repo.Add(MakePassenger());

        repo.GetByEmail("abdullah@example.com").Should().BeNull();
        repo.GetByEmail("mahmoud@example.com").Should().NotBeNull();
    }

    [Fact]
    public void GetAll_MultiplePassengers_ShouldReturnAllOfThem()
    {
        var repo = new FilePassengerRepository(_dataDirectory);
        var mahmoud = MakePassenger();
        var abdullah = MakePassenger("P2", "Abdullah", "Noor", "abdullah@example.com");
        var hussam = MakePassenger("P3", "Hussam", "Burqan", "hussam@example.com");
        repo.Add(mahmoud);
        repo.Add(abdullah);
        repo.Add(hussam);

        var result = repo.GetAll().ToList();

        result.Should().HaveCount(3);
        result.Should().Contain([mahmoud, abdullah, hussam]);
        result[0].Should().BeSameAs(mahmoud);
        result[1].Should().BeSameAs(abdullah);
        result[2].Should().BeSameAs(hussam);
    }
}