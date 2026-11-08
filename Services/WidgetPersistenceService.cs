using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;

namespace Win11Widgets.Services;

public class WidgetPersistenceService
{
    private readonly ILogger<WidgetPersistenceService> _logger;
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public WidgetPersistenceService(ILogger<WidgetPersistenceService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Win11Widgets",
            "Widgets");
        
        Directory.CreateDirectory(_dataDirectory);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }

    public void SaveWidgetState(WidgetState state)
    {
        try
        {
            var fileName = $"{state.Id}.json";
            var filePath = Path.Combine(_dataDirectory, fileName);
            var json = JsonSerializer.Serialize(state, state.GetType(), _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save widget state for {Id}", state.Id);
        }
    }

    public T? LoadWidgetState<T>(string widgetId) where T : WidgetState
    {
        try
        {
            var fileName = $"{widgetId}.json";
            var filePath = Path.Combine(_dataDirectory, fileName);
            
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load widget state for {Id}", widgetId);
            return null;
        }
    }

    public List<WidgetState> LoadAllWidgetStates()
    {
        var states = new List<WidgetState>();
        
        try
        {
            var files = Directory.GetFiles(_dataDirectory, "*.json");
            
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var baseState = JsonSerializer.Deserialize<WidgetState>(json, _jsonOptions);
                    
                    if (baseState != null)
                    {
                        var typedState = DeserializeByType(baseState, json);
                        if (typedState != null)
                        {
                            states.Add(typedState);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load widget from {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load widget states");
        }

        return states.OrderBy(s => s.ZIndex).ToList();
    }

    private WidgetState? DeserializeByType(WidgetState baseState, string json)
    {
        return baseState.Type switch
        {
            WidgetType.StickyNote => JsonSerializer.Deserialize<StickyNoteWidgetState>(json, _jsonOptions),
            WidgetType.Clock => JsonSerializer.Deserialize<ClockWidgetState>(json, _jsonOptions),
            WidgetType.Weather => JsonSerializer.Deserialize<WeatherWidgetState>(json, _jsonOptions),
            WidgetType.SystemMonitor => JsonSerializer.Deserialize<SystemMonitorWidgetState>(json, _jsonOptions),
            WidgetType.TodoList => JsonSerializer.Deserialize<TodoListWidgetState>(json, _jsonOptions),
            WidgetType.Calendar => JsonSerializer.Deserialize<CalendarWidgetState>(json, _jsonOptions),
            WidgetType.Calculator => JsonSerializer.Deserialize<CalculatorWidgetState>(json, _jsonOptions),
            WidgetType.ColorPicker => JsonSerializer.Deserialize<ColorPickerWidgetState>(json, _jsonOptions),
            _ => baseState
        };
    }

    public bool DeleteWidgetState(string widgetId)
    {
        try
        {
            var fileName = $"{widgetId}.json";
            var filePath = Path.Combine(_dataDirectory, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete widget state for {Id}", widgetId);
        }
        return false;
    }

    public void ClearAllWidgets()
    {
        try
        {
            var files = Directory.GetFiles(_dataDirectory, "*.json");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all widgets");
        }
    }
}