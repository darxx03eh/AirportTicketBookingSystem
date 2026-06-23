using System.Text.Json;
using System.Xml.Linq;
using AirportTicketBookingSystem.Infrastructure.IRepositories;
using AirportTicketBookingSystem.Models.Entities;

namespace AirportTicketBookingSystem.Infrastructure.Repositories;

public class FileBookingRepository : IBookingRepository
{
    private readonly string _filePath;
    private List<Booking> _cache = new();

    public FileBookingRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, "bookings.json");
        EnsureFileExists();
        Load();
    }
    
    public IEnumerable<Booking> GetAll() => _cache;
    public Booking? GetById(string id) => _cache.FirstOrDefault(booking => booking.Id == id);
    public IEnumerable<Booking> GetByPassengerId(string passengerId) 
        => _cache.Where(booking => booking.PassengerId == passengerId);

    public void Add(Booking booking)
    {
        _cache.Add(booking);
        Save();
    }
    public void Update(Booking booking)
    {
        var index = _cache.FindIndex(b => b.Id == booking.Id);
        if (index >= 0)
            _cache[index] = booking;
        Save();
    }

    public void Delete(string id)
    {
        _cache.RemoveAll(booking => booking.Id == id);
        Save();
    }
    
    private void Load()
    {
        var json = File.ReadAllText(_filePath);
        _cache = JsonSerializer.Deserialize<List<Booking>>(json) ?? new List<Booking>();
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