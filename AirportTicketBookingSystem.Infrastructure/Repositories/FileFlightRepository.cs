using System.Text.Json;
using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Repositories;

public class FileFlightRepository : IFlightRepository
{
    private readonly string _filePath;
    private List<Flight> _cache = new();

    public FileFlightRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "flights.json");
        EnsureFileExists();
        Load();
    }

    public IEnumerable<Flight> GetAll() => _cache;

    public Flight? GetById(string id) =>
        _cache.FirstOrDefault(f => f.Id == id);

    public void Add(Flight flight)
    {
        _cache.Add(flight);
        Save();
    }

    public void AddRange(IEnumerable<Flight> flights)
    {
        _cache.AddRange(flights);
        Save();
    }

    public void Delete(string id)
    {
        _cache.RemoveAll(f => f.Id == id);
        Save();
    }

    private void Load()
    {
        var json = File.ReadAllText(_filePath);
        _cache = JsonSerializer.Deserialize<List<Flight>>(json) ?? new List<Flight>();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private void EnsureFileExists()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }
}