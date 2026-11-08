using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Timers;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class ClockWidgetViewModel : WidgetViewModelBase
{
    private Timer? _timer;

    [ObservableProperty]
    private string _currentTime = "";

    [ObservableProperty]
    private string _currentDate = "";

    public override WidgetType WidgetType => WidgetType.Clock;

    public ClockWidgetViewModel(
        ILogger<ClockWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override void LoadState(WidgetState state)
    {
        if (state is ClockWidgetState clockState)
        {
            UpdateTime();
        }
        StartTimer();
    }

    private void StartTimer()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += (s, e) => UpdateTime();
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void UpdateTime()
    {
        var now = DateTime.Now;
        var state = State as ClockWidgetState;
        
        if (state?.Use24HourFormat == true)
        {
            CurrentTime = now.ToString("HH:mm") + (state.ShowSeconds ? $":{now:ss}" : "");
        }
        else
        {
            CurrentTime = now.ToString("hh:mm tt") + (state?.ShowSeconds == true ? $":{now:ss}" : "");
        }

        if (state?.ShowDate == true)
        {
            CurrentDate = now.ToString("dddd, MMMM dd, yyyy");
        }
        else
        {
            CurrentDate = "";
        }
    }

    protected override void Cleanup()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }
}