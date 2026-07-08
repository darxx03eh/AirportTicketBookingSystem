using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Service.Implementations;
using AirportTicketBookingSystem.Service.Interfaces;
using AirportTicketBookingSystem.Service.TestModels.Services;
using AirportTicketBookingSystem.Tests.Attributes;
using FluentAssertions;
using Moq;
using static AirportTicketBookingSystem.Tests.Utils.Helpers.PassengerServiceTestHelpers;
namespace AirportTicketBookingSystem.Tests.Services;

public class PassengerServiceTests
{
    private readonly Mock<IPassengerRepository> _passengerRepository = new();
    private readonly IPassengerService _sut;
    public PassengerServiceTests() => _sut = new PassengerService(_passengerRepository.Object);

    [Fact]
    public void GetOrCreate_ExistingEmail_ShouldReturnExistingPassengerWithoutAdding()
    {
        var existing = MakePassenger();
        _passengerRepository.Setup(p => p.GetByEmail("mahmoud@example.com"))
            .Returns(existing);

        var result = _sut.GetOrCreate("Mahmoud", "Darawsheh", "mahmoud@example.com");

        result.Should().BeSameAs(existing);
        _passengerRepository.Verify(p => p.Add(It.IsAny<Passenger>()), Times.Never);
    }

    [Fact]
    public void GetOrCreate_ExistingEmail_ShouldIgnorePassedInNameEvenIfDifferent()
    {
        var existing = MakePassenger();
        _passengerRepository.Setup(p => p.GetByEmail(existing.Email))
            .Returns(existing);

        var result = _sut.GetOrCreate("Mohammad", "Awad", "mahmoud@example.com");
        
        result.FirstName.Should().Be(existing.FirstName);
        result.LastName.Should().Be(existing.LastName);
        result.FullName.Should().Be(existing.FullName);
        _passengerRepository.Verify(p => p.Add(It.IsAny<Passenger>()), Times.Never);
    }

    [Fact]
    public void GetOrCreate_ShouldCreateAndReturnsNewPassenger_WhenNoExistingEmail()
    {
        _passengerRepository.Setup(p => p.GetByEmail("new@example.com"))
            .Returns((Passenger?)null);

        var result = _sut.GetOrCreate("Mahmoud", "Darawsheh", "new@example.com");
        
        result.FirstName.Should().Be("Mahmoud");
        result.LastName.Should().Be("Darawsheh");
        result.Email.Should().Be("new@example.com");
        result.FullName.Should().Be("Mahmoud Darawsheh");
        _passengerRepository.Verify(p => p.Add(It.Is<Passenger>(
            x => Equals(x.FirstName, "Mahmoud") &&
                 Equals(x.LastName, "Darawsheh") &&
                 Equals(x.Email, "new@example.com")
            )), Times.Once);
    }

    [Fact]
    public void GetOrCreate_ShouldReturnsTheSameInstancePassedToAdd()
    {
        _passengerRepository.Setup(p => p.GetByEmail("new@example.com"))
            .Returns((Passenger?)null);

        Passenger? added = null;
        _passengerRepository.Setup(p => p.Add(It.IsAny<Passenger>()))
            .Callback<Passenger>(x => added = x);

        var result = _sut.GetOrCreate("Mahmoud", "Darawsheh", "new@example.com");

        result.Should().BeSameAs(added);
    }

    [Theory]
    [CsvData(typeof(PassengerTestCaseRow), "TestData", "Services", "passenger-info-tests-data.csv")]
    public void GetOrCreate_ShouldStillPassesThemThroughAsIs_WhenNullArguments(PassengerTestCaseRow testCase)
    {
        _passengerRepository.Setup(p => p.GetByEmail(testCase.Email!))
            .Returns((Passenger?)null);

        var result = _sut.GetOrCreate(testCase.FirstName, testCase.LastName, testCase.Email);
        
        result.FirstName.Should().Be(testCase.FirstName);
        result.LastName.Should().Be(testCase.LastName);
        result.Email.Should().Be(testCase.Email);
    }

    [Fact]
    public void GetById_ShouldReturnsPassenger_WhenIdFound()
    {
        var passenger =  MakePassenger();
        _passengerRepository.Setup(p => p.GetById("P1"))
            .Returns(passenger);
        
        var result = _sut.GetById("P1");

        result.Should().BeSameAs(passenger);
    }

    [Fact]
    public void GetById_ShouldReturnsNull_WhenIdNotFound()
    {
        _passengerRepository.Setup(p => p.GetById(It.IsAny<string>()))
            .Returns((Passenger?)null);
        
        var result = _sut.GetById("P1");
        
        result.Should().BeNull();
    }
}