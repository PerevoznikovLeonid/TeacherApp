using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace TeacherApp.Services;

public class JsonDataService(string filePath)
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };
    
    public void SaveData<T>(ObservableCollection<T> collection)
    {
        try
        {
            var json = JsonSerializer.Serialize(collection, _options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка сохранения данных: {e.Message}");
            throw;
        }
    }

    public ObservableCollection<T>? LoadData<T>()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            var json = File.ReadAllText(filePath);
            var collection = JsonSerializer.Deserialize<ObservableCollection<T>>(json, _options);
            return collection;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Ошибка загрузки данных: {e.Message}");
            throw;
        }
    }
}