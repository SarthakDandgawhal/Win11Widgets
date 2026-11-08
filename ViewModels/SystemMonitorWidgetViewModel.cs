using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Timers;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class SystemMonitorWidgetViewModel : WidgetViewModelBase
{
    private Timer? _timer;
    private PerformanceCounter? _cpuCounter;
    private readonly List<double> _cpuHistory = new();
    private readonly List<double> _memoryHistory = new();
    private readonly List<double> _diskHistory = new();
    private readonly List<double> _networkHistory = new();

    [ObservableProperty]
    private double _cpuUsage = 0;

    [ObservableProperty]
    private double _memoryUsage = 0;

    [ObservableProperty]
    private double _diskUsage = 0;

    [ObservableProperty]
    private double _networkUsage = 0;

    [ObservableProperty]
    private string _cpuText = "0%";

    [ObservableProperty]
    private string _memoryText = "0 GB / 0 GB";

    [ObservableProperty]
    private string _diskText = "0 GB / 0 GB";

    [ObservableProperty]
    private string _networkText = "0 MB/s";

    public IReadOnlyList<double> CpuHistory => _cpuHistory;
    public IReadOnlyList<double> MemoryHistory => _memoryHistory;
    public IReadOnlyList<double> DiskHistory => _diskHistory;
    public IReadOnlyList<double> NetworkHistory => _networkHistory;

    public override WidgetType WidgetType => WidgetType.SystemMonitor;

    public SystemMonitorWidgetViewModel(
        ILogger<SystemMonitorWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override void LoadState(WidgetState state)
    {
        StartMonitoring();
    }

    private void StartMonitoring()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cpuCounter.NextValue(); // First call returns 0
        }
        catch
        {
            _logger.LogWarning("Performance counters not available");
        }

        _timer = new Timer(1000);
        _timer.Elapsed += async (s, e) => await UpdateMetrics();
        _timer.AutoReset = true;
        _timer.Start();
    }

    private async Task UpdateMetrics()
    {
        try
        {
            // CPU
            if (_cpuCounter != null)
            {
                CpuUsage = Math.Round(_cpuCounter.NextValue(), 1);
                CpuText = $"{CpuUsage}%";
                AddToHistory(_cpuHistory, CpuUsage);
            }

            // Memory
            var memInfo = GetMemoryInfo();
            MemoryUsage = memInfo.UsedPercent;
            MemoryText = $"{memInfo.UsedGB:F1} GB / {memInfo.TotalGB:F1} GB";
            AddToHistory(_memoryHistory, MemoryUsage);

            // Disk
            var diskInfo = GetDiskInfo();
            DiskUsage = diskInfo.UsedPercent;
            DiskText = $"{diskInfo.UsedGB:F0} GB / {diskInfo.TotalGB:F0} GB";
            AddToHistory(_diskHistory, DiskUsage);

            // Network (simplified)
            NetworkUsage = GetNetworkUsage();
            NetworkText = $"{NetworkUsage:F1} MB/s";
            AddToHistory(_networkHistory, NetworkUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update system metrics");
        }
    }

    private void AddToHistory(List<double> history, double value)
    {
        history.Add(value);
        if (history.Count > 60) history.RemoveAt(0);
        OnPropertyChanged(nameof(CpuHistory));
        OnPropertyChanged(nameof(MemoryHistory));
        OnPropertyChanged(nameof(DiskHistory));
        OnPropertyChanged(nameof(NetworkHistory));
    }

    private (double UsedPercent, double UsedGB, double TotalGB) GetMemoryInfo()
    {
        var total = GC.GetTotalMemory(false);
        // This is managed memory only - for real app, use PerformanceCounter or WMI
        return (50, 8, 16); // Placeholder
    }

    private (double UsedPercent, double UsedGB, double TotalGB) GetDiskInfo()
    {
        try
        {
            var drive = new DriveInfo("C");
            var total = drive.TotalSize / (1024.0 * 1024 * 1024);
            var free = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
            var used = total - free;
            var percent = (used / total) * 100;
            return (percent, used, total);
        }
        catch
        {
            return (50, 500, 1000);
        }
    }

    private double GetNetworkUsage()
    {
        // Placeholder - real implementation would use NetworkInterface
        return new Random().NextDouble() * 10;
    }

    protected override void Cleanup()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _cpuCounter?.Dispose();
    }
}