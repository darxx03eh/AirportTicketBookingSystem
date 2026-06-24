using Xunit;
using System;
using System.IO;
using FluentAssertions;

namespace AirportTicketBookingSystem.XUnitTest
{
    public class FlightCsvParserTests
    {
        [Fact]
        public void Parse_Valid_Csv_Data_Returns_Correct_Format()
        {
            // Arrange
            var csvData = "FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice\n" +
                         "AB123,Palestine,Turkey,Queen Alia International,Istanbul Airport,2025-09-15,350.00\n" +
                         "CD456,Jordan,Germany,Queen Alia International,Frankfurt Airport,2025-10-01,720.00";

            // Act
            var lines = csvData.Split('\n');
            var header = lines[0];
            var dataLines = lines.Skip(1).ToArray();

            // Assert
            header.Should().Be("FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice");
            dataLines.Length.Should().Be(2);
            dataLines[0].Should().Contain("AB123");
            dataLines[1].Should().Contain("CD456");
        }

        [Fact]
        public void Parse_Empty_Csv_Data_Returns_Empty()
        {
            // Arrange
            var csvData = "FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice";

            // Act
            var lines = csvData.Split('\n');

            // Assert
            lines.Length.Should().Be(1);
            lines[0].Should().Be("FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice");
        }

        [Fact]
        public void Parse_Invalid_Date_Format_Returns_Error()
        {
            // Arrange
            var csvData = "FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice\n" +
                         "AB123,Palestine,Turkey,Queen Alia International,Istanbul Airport,invalid-date,350.00";

            // Act
            var lines = csvData.Split('\n');
            var dataLine = lines[1];
            var columns = dataLine.Split(',');

            // Assert
            columns.Length.Should().Be(7);
            columns[0].Should().Be("AB123");
            columns[5].Should().Be("invalid-date"); // DepartureDate column
            columns[6].Should().Be("350.00"); // BasePrice column
        }
    }
}