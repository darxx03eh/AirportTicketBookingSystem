using Xunit;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using FluentAssertions;

namespace AirportTicketBookingSystem.XUnitTest
{
    public class FlightTests
    {
        [Fact]
        public void Flight_Properties_Are_Correctly_Set()
        {
            // Arrange
            var flight = new Flight
            {
                Id = "test-123",
                FlightNumber = "AB123",
                DepartureCountry = "Palestine",
                DestinationCountry = "Turkey",
                DepartureAirport = "Queen Alia International",
                ArrivalAirport = "Istanbul Airport",
                DepartureDate = new DateTime(2025, 9, 15),
                BasePrice = 350.00m
            };

            // Assert
            flight.Id.Should().Be("test-123");
            flight.FlightNumber.Should().Be("AB123");
            flight.DepartureCountry.Should().Be("Palestine");
            flight.DestinationCountry.Should().Be("Turkey");
            flight.DepartureAirport.Should().Be("Queen Alia International");
            flight.ArrivalAirport.Should().Be("Istanbul Airport");
            flight.DepartureDate.Should().Be(new DateTime(2025, 9, 15));
            flight.BasePrice.Should().Be(350.00m);
        }

        [Fact]
        public void Flight_GetPriceForType_Returns_Correct_Price()
        {
            // Arrange
            var flight = new Flight
            {
                FlightNumber = "AB123",
                BasePrice = 350.00m
            };

            // Act & Assert
            flight.GetPriceForType(FlightType.Economy).Should().Be(350.00m);
            flight.GetPriceForType(FlightType.Business).Should().Be(525.00m); // 350 * 1.5
            flight.GetPriceForType(FlightType.FirstClass).Should().Be(700.00m); // 350 * 2
        }

        [Fact]
        public void Flight_Equality_Comparison_Works()
        {
            // Arrange
            var flight1 = new Flight { Id = "test-123" };
            var flight2 = new Flight { Id = "test-123" };
            var flight3 = new Flight { Id = "test-456" };

            // Assert
            flight1.Should().Be(flight2);
            flight1.Should().NotBe(flight3);
            flight1.GetHashCode().Should().Be(flight2.GetHashCode());
        }
    }
}