using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;

namespace Win11Widgets.Services;

public class SettingsService
{
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private WidgetSettings _settings = new();

    public WidgetSettings Settings => _settings;

    public event Action<WidgetSettings>? SettingsChanged;

    public SettingsService(ILogger<SettingsService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Win11Widgets",
            "settings.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<WidgetSettings>(json, _jsonOptions);
                if (loaded != null)
                {
                    _settings = loaded;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
        }
    }

    public void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            File.WriteAllText(_settingsPath, json);
            SettingsChanged?.Invoke(_settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
        }
    }

    public void UpdateSetting<T>(string propertyName, T value)
    {
        var property = typeof(WidgetSettings).GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(_settings, value);
            SaveSettings();
        }
    }

    public void ResetToDefaults()
    {
        _settings = new WidgetSettings();
        SaveSettings();
    }
}

public class WidgetSettings
{
    public bool AutoStart { get; set; } = false;
    public bool ShowInSystemTray { get; set; } = true;
    public double DefaultWidgetOpacity { get; set; } = 0.95;
    public bool SnapToGrid { get; set; } = true;
    public int GridSize { get; set; } = 20;
    public int WidgetSpacing { get; set; } = 12;
    public bool EnableDropShadow { get; set; } = true;
    public double DefaultWidgetWidth { get; set; } = 320;
    public double DefaultWidgetHeight { get; set; } = 240;
    public string Theme { get; set; } = "System"; // Light, Dark, System
    public bool ShowWidgetBorders { get; set; } = true;
    public int AnimationDurationMs { get; set; } = 200;
    public bool EnableHotkeys { get; set; } = true;
    public string HotkeyToggleOverlay { get; set; } = "Win+Shift+W";
    public string HotkeyNewWidget { get; set; } = "Win+Shift+N";
    public string HotkeySettings { get; set; } = "Win+Shift+S";
}