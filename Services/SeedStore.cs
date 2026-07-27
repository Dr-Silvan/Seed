using System.Text.Json;
using System.IO;
using Seed.Models;

namespace Seed.Services;

public sealed class SeedStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _path;

    public SeedStore()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Seed");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "seed-data.json");
    }

    public AppState Load()
    {
        try
        {
            return File.Exists(_path)
                ? JsonSerializer.Deserialize<AppState>(File.ReadAllText(_path), JsonOptions) ?? new()
                : new();
        }
        catch
        {
            return new();
        }
    }

    public void Save(AppState state) =>
        File.WriteAllText(_path, JsonSerializer.Serialize(state, JsonOptions));
}
