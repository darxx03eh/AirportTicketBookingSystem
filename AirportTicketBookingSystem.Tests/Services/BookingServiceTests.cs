using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Service.DTOs;
using AirportTicketBookingSystem.Service.Implementations;
using AirportTicketBookingSystem.Service.Interfaces;
using FluentAssertions;
using Moq;
using static AirportTicketBookingSystem.Tests.Utils.Helpers.BookingServiceTestHelpers;

namespace AirportTicketBookingSystem.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new();
    private readonly Mock<IFlightRepository> _flightRepository = new();
    private readonly Mock<IPassengerRepository> _passengerRepository = new();
    private readonly IBookingService _sut;
    public BookingServiceTests() 
        => _sut = new BookingService(_bookingRepository.Object, _flightRepository.Object, _passengerRepository.Object);

    [Fact]
    public void CreateBooking_ShouldAddAndReturnsBooking_WhenFlightAndPassengerAreValid()
    {
        var flight = MakeFlight();
        var passenger = MakePassenger();
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns(flight);
        _passengerRepository.Setup(p => p.GetById("P1"))
            .Returns(passenger);

        var result = _sut.CreateBooking("P1", "F1", FlightType.Economy);

        result.FlightId.Should().Be("F1");
        result.PassengerId.Should().Be("P1");
        result.Flight.Should().BeEquivalentTo(flight);
        result.Passenger.Should().BeEquivalentTo(passenger);
        result.Type.Should().Be(FlightType.Economy);
        _bookingRepository.Verify(repo => repo.Add(It.Is<Booking>(
            b => Equals(b.FlightId, "F1") &&
                 Equals(b.PassengerId, "P1") &&
                 Equals(b.Type, FlightType.Economy)
            )), Times.Once);
    }

    [Fact]
    public void CreateBooking_ShouldThrowInvalidOperationException_WhenFlightNotFound()
    {
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns((Flight?)null);
        
        Action act = () => _sut.CreateBooking("P1", "F1", FlightType.Economy);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*F1*");
        _passengerRepository.Verify(p => p.GetById(It.IsAny<string>()), Times.Never);
        _flightRepository.Verify(f => f.GetById(It.IsAny<string>()), Times.Once);
        _bookingRepository.Verify(b => b.Add(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public void CreateBooking_ShouldThrowInvalidOperationException_WhenPassengerNotFound()
    {
        _passengerRepository.Setup(p => p.GetById("P1"))
            .Returns((Passenger?)null);
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns(MakeFlight());
        
        Action act = () => _sut.CreateBooking("P1", "F1", FlightType.Economy);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*P1*");
        _flightRepository.Verify(f => f.GetById(It.IsAny<string>()), Times.Once);
        _passengerRepository.Verify(p => p.GetById(It.IsAny<string>()), Times.Once);
        _bookingRepository.Verify(b => b.Add(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public void CancelBooking_ShouldThrowInvalidOperationException_WhenBookingNotFound()
    {
        _bookingRepository.Setup(b => b.GetById("B1"))
            .Returns((Booking?)null);
        
        Action act = () => _sut.CancelBooking("B1", "P1");
        act.Should().Throw<InvalidOperationException>();
        _bookingRepository.Verify(b => b.Delete(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void CancelBooking_ShouldDeleteBooking_WhenPassengerOwnedIt()
    {
        _bookingRepository.Setup(b => b.GetById("B1"))
            .Returns(MakeBooking());
        
        _sut.CancelBooking("B1", "P1");
        _bookingRepository.Verify(b => b.Delete("B1"), Times.Once);
    }

    [Fact]
    public void CancelBooking_ShouldThrowUnauthorizedAccessException_WhenPassengerNotOwnedIt()
    {
        _bookingRepository.Setup(p => p.GetById("B1"))
            .Returns(MakeBooking(passengerId: "P2"));
        
        Action act = () => _sut.CancelBooking("B1", "P1");
        act.Should().Throw<UnauthorizedAccessException>();
        _bookingRepository.Verify(b => b.Delete(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ModifyBooking_ShouldUpdatesFlightIdAndPersists_WhenProvidesNewFlightId()
    {
        var booking =  MakeBooking();
        var newFlight = MakeFlight("F2");
        _bookingRepository.Setup(b => b.GetById("B1"))
            .Returns(booking);
        _flightRepository.Setup(f => f.GetById("F2"))
            .Returns(newFlight);

        var result = _sut.ModifyBooking("B1", "P1", "F2", FlightType.FirstClass);
        
        result.FlightId.Should().Be("F2");
        result.PassengerId.Should().Be("P1");
        result.Type.Should().Be(FlightType.FirstClass);
        result.Flight.Should().BeEquivalentTo(newFlight);
        _bookingRepository.Verify(b => b.Update(booking), Times.Once);
    }

    [Fact]
    public void ModifyBooking_ShouldUpdateOnlyType_WhenTypeOnlyProvided()
    {
        var booking = MakeBooking();
        _bookingRepository.Setup(b => b.GetById("B1"))
            .Returns(booking);

        var result = _sut.ModifyBooking("B1", "P1", null, FlightType.Business);
        
        result.Type.Should().Be(FlightType.Business);
        result.FlightId.Should().Be("F1");
        _flightRepository.Verify(f => f.GetById(It.IsAny<string>()), Times.Never);
        _bookingRepository.Verify(b => b.Update(booking), Times.Once);
    }

    [Fact]
    public void ModifyBooking_ShouldThrowInvalidOperationException_WhenNewFlightIsNotFound()
    {
        var booking =  MakeBooking();
        _flightRepository.Setup(f => f.GetById("F2"))
            .Returns((Flight?)null);
        _bookingRepository.Setup(b => b.GetById("B1"))
            .Returns(booking);
        
        Action act = () => _sut.ModifyBooking("B1", "P1", "F2", FlightType.Business);
        
        act.Should().Throw<InvalidOperationException>();
        _bookingRepository.Verify(b => b.Update(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public void ModifyBooking_ShouldThrowUnauthorizedAccessException_WhenPassengerNotOwnedIt()
    {
        var booking = MakeBooking(passengerId: "P2");
        _bookingRepository.Setup(b => b.GetById("B1"))
            .Returns(booking);
        
        Action act = () => _sut.ModifyBooking("B1", "P1", null, FlightType.Business);

        act.Should().Throw<UnauthorizedAccessException>();
        _bookingRepository.Verify(b => b.Update(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public void GetPassengerBookings_ShouldEnrichesFlightAndPassenger_WhenMissing()
    {
        var booking  = MakeBooking();
        _bookingRepository.Setup(b => b.GetByPassengerId("P1"))
            .Returns([booking]);
        _passengerRepository.Setup(p => p.GetById("P1"))
            .Returns(MakePassenger());
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns(MakeFlight());

        var result = _sut.GetPassengerBookings("P1").ToList();

        result.Should().ContainSingle();
        result[0].Flight.Should().NotBeNull();
        result[0].Passenger.Should().NotBeNull();
        _flightRepository.Verify(f => f.GetById(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetPassengerBookings_ShouldDoesNotOverWrite_AlreadyLoadedNavigationProps()
    {
        var flight =  MakeFlight();
        var booking =  MakeBooking();
        booking.Flight = flight;
        _bookingRepository.Setup(b => b.GetByPassengerId("P1"))
            .Returns([booking]);
        _passengerRepository.Setup(p => p.GetById("P1"))
            .Returns(MakePassenger());

        var result = _sut.GetPassengerBookings("P1").ToList();
        
        result.Should().ContainSingle();
        result[0].Flight.Should().NotBeNull();
        result[0].Passenger.Should().NotBeNull();
        _flightRepository.Verify(f => f.GetById(It.IsAny<string>()), Times.Never);
        _passengerRepository.Verify(p => p.GetById("P1"), Times.Once);
    }

    [Fact]
    public void FilterBookings_ByFlightNumber_ShouldReturnsMatchesCaseInsensitive()
    {
        var match = MakeBooking();
        var noMatch = MakeBooking("B2", "F2", "P2");
        var flight = MakeFlight();
        _bookingRepository.Setup(b => b.GetAll())
            .Returns([match, noMatch]);
        _flightRepository.Setup(f => f.GetById("F1"))
            .Returns(flight);
        var otherFlight = MakeFlight("F2");
        otherFlight.FlightNumber = "ZZ999";
        _flightRepository.Setup(f => f.GetById("F2"))
            .Returns(otherFlight);
        _passengerRepository.Setup(p => p.GetById(It.IsAny<string>()))
            .Returns(MakePassenger());

        var result = _sut.FilterBookings(new BookingSearchFilter()
        {
            FlightNumber = "ab123"
        }).ToList();
        var booking = result.First();
        
        result.Should().ContainSingle();
        result.Should().NotContain(noMatch);
        booking.Id.Should().Be("B1");
        booking.FlightId.Should().Be("F1");
        booking.Flight.Should().BeEquivalentTo(flight);
        booking.Should().BeEquivalentTo(booking);
    }

    [Fact]
    public void FilterBookings_ByMaxPrice_ShouldExcludesMoreExpensiveBookings()
    {
        var flight = MakeFlight();
        flight.BasePrice = 100.0m;
        var cheapBooking = MakeBooking();
        cheapBooking.Type = FlightType.Economy;
        var expensiveBooking = MakeBooking("B2");
        expensiveBooking.Type = FlightType.Business;
        _bookingRepository.Setup(b => b.GetAll())
            .Returns([cheapBooking, expensiveBooking]);
        _flightRepository.Setup(f => f.GetById(It.IsAny<string>()))
            .Returns(flight);
        _passengerRepository.Setup(p => p.GetById(It.IsAny<string>()))
            .Returns(MakePassenger());

        var result = _sut.FilterBookings(new BookingSearchFilter()
        {
            MaxPrice = 100.0m
        }).ToList();
        var booking = result.First();

        result.Should().ContainSingle();
        result.Should().Contain(cheapBooking);
        result.Should().NotContain(expensiveBooking);
        booking.Id.Should().Be("B1");
    }

    [Fact]
    public void FilterBookings_ByType_ShouldReturnOnlyMatchingType()
    {
        var economy = MakeBooking();
        var business = MakeBooking("B2");
        business.Type = FlightType.Business;
        _bookingRepository.Setup(b => b.GetAll())
            .Returns([economy, business]);
        _flightRepository.Setup(f => f.GetById(It.IsAny<string>()))
            .Returns(MakeFlight());
        _passengerRepository.Setup(p => p.GetById(It.IsAny<string>()))
            .Returns(MakePassenger());

        var result = _sut.FilterBookings(new BookingSearchFilter()
        {
            Type = FlightType.Business
        }).ToList();
        var booking = result.First();
        
        result.Should().ContainSingle();
        result.Should().Contain(business);
        result.Should().NotContain(economy);
        booking.Id.Should().Be("B2");
    }

    [Fact]
    public void FilterBookings_ShouldReturnAllBookings_WhenNoFilterProvided()
    {
        var bookings = new[] { MakeBooking(), MakeBooking("B2") };
        _bookingRepository.Setup(b => b.GetAll())
            .Returns(bookings);
        _flightRepository.Setup(f => f.GetById(It.IsAny<string>()))
            .Returns(MakeFlight());
        _passengerRepository.Setup(p => p.GetById(It.IsAny<string>()))
            .Returns(MakePassenger());

        var result = _sut.FilterBookings(new BookingSearchFilter())
            .ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(bookings);
        result[0].Id.Should().Be("B1");
        result[1].Id.Should().Be("B2");
    }
}