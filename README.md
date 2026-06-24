# Airport Ticket Booking System

A comprehensive airline ticket booking application built with .NET 10.0 and Spectre.Console for a rich terminal user interface.

## Overview

The Airport Ticket Booking System enables passengers and managers to search, book, and manage flight tickets through an intuitive terminal-based interface. The system provides a complete end-to-end solution for airline ticket booking with features for both regular passengers and administrative staff.

## Key Features

### For Passengers
- **Flight Search**: Search flights by route, departure date, and price range
- **Flight Display**: View available flights with prices for all classes (Economy, Business, First Class)
- **Booking Management**: Book flights, view personal bookings, cancel and modify existing bookings
- **Passenger Management**: Login or register with email to create passenger accounts

### For Managers
- **Booking Management**: Filter and view all bookings across the system
- **Flight Import**: Import flight data from CSV files with validation
- **Validation Rules**: View and manage flight validation rules
- **Simple Authentication**: Password-based manager access

### Technical Features
- **Repository Pattern**: Clean separation of data access logic
- **Comprehensive Validation**: Custom validation attributes for data integrity
- **CSV Import/Export**: Import flight data with error handling
- **Rich Terminal UI**: Built with Spectre.Console for professional appearance
- **Unit Testing**: Comprehensive test coverage with xUnit and FluentAssertions

## Architecture

The project follows a layered architecture with clear separation of concerns:

```
AirportTicketBookingSystem/
├── AirportTicketBookingSystem.Models/           # Domain models and entities
│   ├── Entities/                              # Core business entities
│   ├── Enums/                                 # Enumerations
│   ├── Attributes/                            # Custom validation attributes
│   └── DTOs/                                  # Data transfer objects
├── AirportTicketBookingSystem.Infrastructure/   # Data access and parsing
│   ├── IParsers/                              # Parser interfaces
│   ├── Parsers/                               # Parser implementations
│   ├── IRepositories/                         # Repository interfaces
│   └── Repositories/                          # Repository implementations
├── AirportTicketBookingSystem.Service/         # Business logic
│   ├── Interfaces/                            # Service interfaces
│   ├── Implementations/                       # Service implementations
│   └── DTOs/                                  # Service DTOs
├── AirportTicketBookingSystem.UI/              # User interface
│   ├── IMenus/                                # Menu interfaces
│   ├── Menus/                                 # Menu implementations
│   └── Displays/                              # Display components
├── AirportTicketBookingSystem.XUnitTest/       # Tests
│   ├── FlightTests.cs
│   ├── BookingTests.cs
│   └── FlightCsvParserTests.cs
└── Program.cs                                 # Application entry point
```

## Project Structure

### Models Layer
- **Entities**: Flight, Booking, Passenger
- **Enums**: FlightType (Economy, Business, FirstClass), UserRole (Passenger, Manager)
- **Attributes**: FutureDateAttribute, PositiveDecimalAttribute
- **DTOs**: ImportError (for CSV import errors)

### Infrastructure Layer
- **Parsers**: FlightCsvParser for CSV file parsing
- **Repositories**: File-based repositories using JSON storage
  - FileFlightRepository (flights.json)
  - FileBookingRepository (bookings.json)
  - FilePassengerRepository (passengers.json)

### Service Layer
- **BookingService**: Handles booking operations (create, cancel, modify)
- **FlightService**: Handles flight operations (search, import, validation)
- **PassengerService**: Handles passenger operations (login, register)

### UI Layer
- **Menus**: ManagerMenu and PassengerMenu with role-based interfaces
- **Displays**: BookingDisplay, FlightDisplay, ValidationDisplay
- **Framework**: Spectre.Console for rich terminal UI

## Technology Stack

### Core Technologies
- **Language**: C# (.NET 10.0)
- **UI Framework**: Spectre.Console
- **Testing**: xUnit, FluentAssertions
- **Persistence**: JSON files

### Dependencies
- `Microsoft.NET.Sdk` (build)
- `Spectre.Console` (UI)
- `FluentAssertions` (testing)
- `System.Text.Json` (JSON serialization)

## Setup and Installation

### Prerequisites
- .NET 10.0 SDK or later
- Terminal/command line access

### Building the Project
1. Clone the repository
2. Open the solution in Visual Studio, Rider, or your preferred IDE
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the solution

### Running Tests
```bash
dotnet test
```

**Expected Result**: All 9 tests pass (FlightTests: 3, BookingTests: 3, FlightCsvParserTests: 3)

### Running the Application
```bash
dotnet run --project AirportTicketBookingSystem.csproj
```

## Usage

### Application Flow

1. **Welcome Screen**: The application starts with a role selection:
   - **Passenger**: Access booking features
   - **Manager**: Access administrative features
   - **Exit**: Terminate the application

2. **Passenger Portal**:
   - **Login/Register**: Enter passenger details to login or create account
   - **Search & Book**: Search flights with filters, view prices by class
   - **View Bookings**: See all personal bookings
   - **Cancel/Modify**: Cancel or modify existing bookings
   - **Logout**: Return to role selection

3. **Manager Portal**:
   - **Authenticate**: Enter manager password (default: `admin123`)
   - **Filter Bookings**: Search and filter all bookings
   - **Import Flights**: Upload CSV files with flight data
   - **View Validation Rules**: See flight validation requirements
   - **Logout**: Return to role selection

### Flight Pricing
- **Economy**: Base price
- **Business**: 1.5x base price
- **First Class**: 2x base price

### Data Storage
All data is stored in JSON files within the `AirportTicketBookingSystem.Infrastructure/Data` directory:
- `flights.json`: Flight data
- `bookings.json`: Booking data
- `passengers.json`: Passenger data

## Flight Search Filters

### Passenger Search
- Departure Country
- Destination Country
- Departure Airport
- Arrival Airport
- Departure Date
- Maximum Price
- Preferred Class (Any, Economy, Business, FirstClass)

### Manager Booking Filter
- Flight Number
- Passenger Name
- Departure Country
- Destination Country
- Departure Airport
- Arrival Airport
- Departure Date
- Maximum Price
- Class Filter

## CSV Import

### Expected Format
```csv
FlightNumber,DepartureCountry,DestinationCountry,DepartureAirport,ArrivalAirport,DepartureDate,BasePrice
AB123,Palestine,Turkey,Queen Alia International,Istanbul Airport,2026-09-15,350.00
CD456,Jordan,Germany,Queen Alia International,Frankfurt Airport,2026-10-01,720.00
```

### Validation Rules
- Flight Number: Format like 'AB123' (2 letters + 3-4 digits)
- Departure Date: Must be today or in the future
- Base Price: Must be greater than 0

## Testing

### Test Coverage
The project includes comprehensive unit tests covering:

#### FlightTests.cs (3 tests)
1. **Flight_Properties_Are_Correctly_Set**: Tests flight property assignment
2. **Flight_GetPriceForType_Returns_Correct_Price**: Tests price multipliers
3. **Flight_Equality_Comparison_Works**: Tests equality comparison using Id

#### BookingTests.cs (3 tests)
1. **Booking_Creation_Works**: Tests booking creation with all properties
2. **Booking_Cancellation_Works**: Tests booking cancellation logic
3. **Booking_Modification_Works**: Tests booking modification (flight/class changes)

#### FlightCsvParserTests.cs (3 tests)
1. **Parse_Valid_Csv_Data_Returns_Correct_Format**: Tests CSV parsing
2. **Parse_Empty_Csv_Data_Returns_Empty**: Tests empty CSV handling
3. **Parse_Invalid_Date_Format_Returns_Error**: Tests error handling

### Test Framework
- **xUnit**: Unit testing framework
- **FluentAssertions**: Clean, readable test assertions

### Running Tests
```bash
dotnet test
```

## Project Files

### Key Files
- **Program.cs**: Application entry point and main menu system
- **Flight.cs**: Flight entity with pricing and equality implementation
- **Booking.cs**: Booking entity with computed total price
- **Passenger.cs**: Passenger entity with validation
- **FlightCsvParser.cs**: CSV file parser with validation
- **BookingService.cs**: Business logic for booking operations
- **FlightService.cs**: Business logic for flight operations
- **ManagerMenu.cs**: Manager interface and functionality
- **PassengerMenu.cs**: Passenger interface and functionality

### Configuration
- **AirportTicketBookingSystem.csproj**: Main project configuration
- **.gitignore**: Git ignore file
- **AirportTicketBookingSystem.sln**: Visual Studio solution file

## Development Guidelines

### Coding Standards
- Use descriptive variable and method names
- Follow .NET coding conventions
- Implement proper error handling
- Write clean, maintainable code
- Use dependency injection for service registration

### Testing Best Practices
- Write unit tests for all business logic
- Use FluentAssertions for readable assertions
- Mock dependencies where appropriate
- Ensure 100% test coverage for critical paths

### Code Quality
- Keep classes focused on single responsibilities
- Use interfaces for better testability
- Implement proper validation
- Follow SOLID principles

### Testing Contributions
1. Review existing tests
2. Add tests for edge cases
3. Ensure test coverage is adequate
4. Run tests locally before submitting

## Building and Deployment

### Local Development
1. Clone the repository
2. Restore dependencies: `dotnet restore`
3. Build: `dotnet build`
4. Run tests: `dotnet test`
5. Run application: `dotnet run`

### CI/CD
The project includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that:
- Restores dependencies
- Builds the solution
- Runs all tests
- Ensures code quality


### Performance Improvements
1. **Caching**: Implement caching for frequently accessed data
2. **Async Operations**: Convert synchronous operations to async
3. **Database Optimization**: Optimize database queries
4. **Memory Management**: Implement proper memory management

## Acknowledgments

- **Spectre.Console**: For providing a rich terminal UI framework
- **xUnit**: For unit testing framework
- **FluentAssertions**: For readable test assertions
- **GitHub Actions**: For CI/CD pipeline
- **All Contributors**: For their contributions to the project