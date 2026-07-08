using System.Globalization;
using System.Reflection;
using CsvHelper;
using Xunit.Sdk;

namespace AirportTicketBookingSystem.Tests.Attributes;

public class CsvDataAttribute : DataAttribute
{
    private readonly string _path;
    private readonly Type _type;
    public CsvDataAttribute(Type type, params string[] path)
        => (_type, _path) = (type, Path.Combine(path));
    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, _path));
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csv.GetRecords(_type);
        foreach (var record in records)
            yield return [record];
    }
}