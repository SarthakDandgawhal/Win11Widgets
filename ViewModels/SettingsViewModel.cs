using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private WidgetSettings _settings;

    public SettingsViewModel(SettingsService settingsService, ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        Settings = settingsService.Settings;
        
        _settingsService.SettingsChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(WidgetSettings newSettings)
    {
        Settings = newSettings;
        OnPropertyChanged(nameof(Settings));
    }

    [RelayCommand]
    public void SaveSettings()
    {
        _settingsService.SaveSettings();
    }

    [RelayCommand]
    public void ResetToDefaults()
    {
        _settingsService.ResetToDefaults();
    }

    public IEnumerable<string> Themes => new[] { "Light", "Dark", "System" };
}