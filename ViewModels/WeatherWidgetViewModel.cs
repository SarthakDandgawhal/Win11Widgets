using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class WeatherWidgetViewModel : WidgetViewModelBase
{
    [ObservableProperty]
    private string _temperature = "--°";

    [ObservableProperty]
    private string _condition = "Loading...";

    [ObservableProperty]
    private string _location = "Auto";

    [ObservableProperty]
    private string _icon = "☀️";

    [ObservableProperty]
    private List<ForecastItem> _forecast = new();

    [ObservableProperty]
    private bool _isLoading = false;

    public override WidgetType WidgetType => WidgetType.Weather;

    public WeatherWidgetViewModel(
        ILogger<WeatherWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override async void LoadState(WidgetState state)
    {
        if (state is WeatherWidgetState weatherState)
        {
            Location = weatherState.Location;
            await RefreshWeather();
        }
    }

    [RelayCommand]
    public async Task RefreshWeather()
    {
        IsLoading = true;
        try
        {
            // Simulated weather data - in real app, call weather API
            await Task.Delay(500);
            
            var rand = new Random();
            var temps = new[] { "18°", "22°", "25°", "28°", "15°", "30°" };
            var conditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy", "Windy" };
            var icons = new[] { "☀️", "☁️", "🌧️", "⛅", "💨" };
            
            Temperature = temps[rand.Next(temps.Length)];
            Condition = conditions[rand.Next(conditions.Length)];
            Icon = icons[rand.Next(icons.Length)];
            
            Forecast = Enumerable.Range(1, 3).Select(i => new ForecastItem
            {
                Day = DateTime.Now.AddDays(i).ToString("ddd"),
                High = $"{rand.Next(20, 32)}°",
                Low = $"{rand.Next(10, 20)}°",
                Icon = icons[rand.Next(icons.Length)],
                Condition = conditions[rand.Next(conditions.Length)]
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch weather");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public partial class ForecastItem : ObservableObject
{
    [ObservableProperty] public string Day = "";
    [ObservableProperty] public string High = "";
    [ObservableProperty] public string Low = "";
    [ObservableProperty] public string Icon = "";
    [ObservableProperty] public string Condition = "";
}