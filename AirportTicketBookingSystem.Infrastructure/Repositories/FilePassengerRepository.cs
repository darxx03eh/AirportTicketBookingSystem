using System.Text.Json;
using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Repositories;

public class FilePassengerRepository : IPassengerRepository
{
    private readonly string _filePath;
    private List<Passenger> _cache = new();

    public FilePassengerRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "passengers.json");
        EnsureFileExists();
        Load();
    }

    public IEnumerable<Passenger> GetAll() => _cache;

    public Passenger? GetById(string id) =>
        _cache.FirstOrDefault(p => p.Id == id);

    public Passenger? GetByEmail(string email) =>
        _cache.FirstOrDefault(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

    public void Add(Passenger passenger)
    {
        _cache.Add(passenger);
        Save();
    }

    private void Load()
    {
        var json = File.ReadAllText(_filePath);
        _cache = JsonSerializer.Deserialize<List<Passenger>>(json) ?? new List<Passenger>();
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