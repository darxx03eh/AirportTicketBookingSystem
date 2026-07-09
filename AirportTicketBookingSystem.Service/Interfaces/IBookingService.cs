using AirportTicketBookingSystem.Models.Entities;
using AirportTicketBookingSystem.Models.Enums;
using AirportTicketBookingSystem.Service.DTOs;

namespace AirportTicketBookingSystem.Service.Interfaces;

public interface IBookingService
{
    Booking CreateBooking(string passengerId, string flightId, FlightType type);
    void CancelBooking(string bookingId, string passengerId);
    Booking ModifyBooking(string bookingId, string passengerId, string? newFlightId, FlightType? newType);
    IEnumerable<Booking> GetPassengerBookings(string passengerId);
    IEnumerable<Booking> FilterBookings(BookingSearchFilter filter);
}