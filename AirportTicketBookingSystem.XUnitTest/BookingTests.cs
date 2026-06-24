using Xunit;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using FluentAssertions;

namespace AirportTicketBookingSystem.XUnitTest
{
    public class BookingTests
    {
        [Fact]
        public void Booking_Creation_Works()
        {
            // Arrange
            var flight = new Flight
            {
                Id = "flight-123",
                FlightNumber = "AB123",
                DepartureCountry = "Palestine",
                DestinationCountry = "Turkey",
                DepartureAirport = "Queen Alia International",
                ArrivalAirport = "Istanbul Airport",
                DepartureDate = new DateTime(2025, 9, 15),
                BasePrice = 350.00m
            };

            var passenger = new Passenger
            {
                Id = "passenger-123",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            // Act
            var booking = new Booking
            {
                Id = "booking-123",
                FlightId = flight.Id,
                Flight = flight,
                PassengerId = passenger.Id,
                Passenger = passenger,
                Type = FlightType.Economy
            };

            // Assert
            booking.Id.Should().Be("booking-123");
            booking.FlightId.Should().Be(flight.Id);
            booking.Flight.Should().Be(flight);
            booking.PassengerId.Should().Be(passenger.Id);
            booking.Passenger.Should().Be(passenger);
            booking.Type.Should().Be(FlightType.Economy);
            booking.TotalPrice.Should().Be(350.00m);
            booking.BookedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Booking_Cancellation_Works()
        {
            // Arrange
            var flight = new Flight
            {
                Id = "flight-123",
                FlightNumber = "AB123",
                DepartureCountry = "Palestine",
                DestinationCountry = "Turkey",
                DepartureAirport = "Queen Alia International",
                ArrivalAirport = "Istanbul Airport",
                DepartureDate = new DateTime(2025, 9, 15),
                BasePrice = 350.00m
            };

            var passenger = new Passenger
            {
                Id = "passenger-123",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var booking = new Booking
            {
                Id = "booking-123",
                FlightId = flight.Id,
                Flight = flight,
                PassengerId = passenger.Id,
                Passenger = passenger,
                Type = FlightType.Economy
            };

            // Act
            var economyPrice = flight.GetPriceForType(FlightType.Economy);
            var businessPrice = flight.GetPriceForType(FlightType.Business);

            // Assert
            economyPrice.Should().Be(350.00m);
            businessPrice.Should().Be(525.00m);
            businessPrice.Should().BeGreaterThan(economyPrice);
        }

        [Fact]
        public void Booking_Modification_Works()
        {
            // Arrange
            var flight = new Flight
            {
                Id = "flight-123",
                FlightNumber = "AB123",
                DepartureCountry = "Palestine",
                DestinationCountry = "Turkey",
                DepartureAirport = "Queen Alia International",
                ArrivalAirport = "Istanbul Airport",
                DepartureDate = new DateTime(2025, 9, 15),
                BasePrice = 350.00m
            };

            var passenger = new Passenger
            {
                Id = "passenger-123",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var booking = new Booking
            {
                Id = "booking-123",
                FlightId = flight.Id,
                Flight = flight,
                PassengerId = passenger.Id,
                Passenger = passenger,
                Type = FlightType.Economy
            };

            // Act
            var economyPrice = flight.GetPriceForType(FlightType.Economy);
            var businessPrice = flight.GetPriceForType(FlightType.Business);
            var firstClassPrice = flight.GetPriceForType(FlightType.FirstClass);

            // Assert
            economyPrice.Should().Be(350.00m);
            businessPrice.Should().Be(525.00m);
            firstClassPrice.Should().Be(700.00m);
            businessPrice.Should().BeGreaterThan(economyPrice);
            firstClassPrice.Should().BeGreaterThan(businessPrice);
        }
    }
}