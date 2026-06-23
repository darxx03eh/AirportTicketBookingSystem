using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Service.DTOs;
using AirportTicketBookingSystem.Service.Interfaces;

namespace AirportTicketBookingSystem.Service.Implementations;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IPassengerRepository _passengerRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IPassengerRepository passengerRepository)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _passengerRepository = passengerRepository;
    }

    public Booking CreateBooking(string passengerId, string flightId, FlightType flightType)
    {
        var flight = _flightRepository.GetById(flightId)
            ?? throw new InvalidOperationException($"Flight '{flightId}' not found.");

        var passenger = _passengerRepository.GetById(passengerId)
            ?? throw new InvalidOperationException($"Passenger '{passengerId}' not found.");

        var booking = new Booking
        {
            FlightId = flightId,
            Flight = flight,
            PassengerId = passengerId,
            Passenger = passenger,
            Type = flightType
        };

        _bookingRepository.Add(booking);
        return booking;
    }

    public void CancelBooking(string bookingId, string passengerId)
    {
        var booking = GetBookingForPassenger(bookingId, passengerId);
        _bookingRepository.Delete(booking.Id);
    }

    public Booking ModifyBooking(string bookingId, string passengerId, string? newFlightId, FlightType? newType)
    {
        var booking = GetBookingForPassenger(bookingId, passengerId);

        if (newFlightId != null)
        {
            var flight = _flightRepository.GetById(newFlightId)
                ?? throw new InvalidOperationException($"Flight '{newFlightId}' not found.");
            booking.FlightId = newFlightId;
            booking.Flight = flight;
        }

        if (newType.HasValue)
            booking.Type = newType.Value;

        _bookingRepository.Update(booking);
        return booking;
    }

    public IEnumerable<Booking> GetPassengerBookings(string passengerId) =>
        _bookingRepository.GetByPassengerId(passengerId)
            .Select(EnrichBooking);

    public IEnumerable<Booking> FilterBookings(BookingSearchFilter filter)
    {
        var bookings = _bookingRepository.GetAll().Select(EnrichBooking);

        if (!string.IsNullOrWhiteSpace(filter.FlightNumber))
            bookings = bookings.Where(b => b.Flight?.FlightNumber.Contains(filter.FlightNumber, StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrWhiteSpace(filter.PassengerName))
            bookings = bookings.Where(b => b.Passenger?.FullName.Contains(filter.PassengerName, StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrWhiteSpace(filter.DepartureCountry))
            bookings = bookings.Where(b => b.Flight?.DepartureCountry.Contains(filter.DepartureCountry, StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrWhiteSpace(filter.DestinationCountry))
            bookings = bookings.Where(b => b.Flight?.DestinationCountry.Contains(filter.DestinationCountry, StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrWhiteSpace(filter.DepartureAirport))
            bookings = bookings.Where(b => b.Flight?.DepartureAirport.Contains(filter.DepartureAirport, StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrWhiteSpace(filter.ArrivalAirport))
            bookings = bookings.Where(b => b.Flight?.ArrivalAirport.Contains(filter.ArrivalAirport, StringComparison.OrdinalIgnoreCase) == true);

        if (filter.DepartureDate.HasValue)
            bookings = bookings.Where(b => b.Flight?.DepartureDate.Date == filter.DepartureDate.Value.Date);

        if (filter.MaxPrice.HasValue)
            bookings = bookings.Where(b => b.TotalPrice <= filter.MaxPrice.Value);

        if (filter.Type.HasValue)
            bookings = bookings.Where(b => b.Type == filter.Type.Value);

        return bookings;
    }

    private Booking GetBookingForPassenger(string bookingId, string passengerId)
    {
        var booking = _bookingRepository.GetById(bookingId)
            ?? throw new InvalidOperationException($"Booking '{bookingId}' not found.");

        if (booking.PassengerId != passengerId)
            throw new UnauthorizedAccessException("You do not have permission to modify this booking.");

        return booking;
    }

    private Booking EnrichBooking(Booking booking)
    {
        booking.Flight ??= _flightRepository.GetById(booking.FlightId);
        booking.Passenger ??= _passengerRepository.GetById(booking.PassengerId);
        return booking;
    }
}
