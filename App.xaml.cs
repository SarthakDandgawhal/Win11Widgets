using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Win11Widgets.Models;
using Win11Widgets.Services;
using Win11Widgets.Views;
using Win11Widgets.ViewModels;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Win11Widgets;

public partial class App : Application
{
    private IHost? _host;
    private WidgetManagerService? _widgetManager;
    private SystemTrayService? _systemTray;
    private OverlayWindow? _overlayWindow;
    private SettingsWindow? _settingsWindow;
    private WidgetGalleryWindow? _galleryWindow;

    public static App Current => (App)Application.Current;
    public IServiceProvider Services => _host!.Services;
    public WidgetManagerService WidgetManager => _widgetManager!;
    public SystemTrayService SystemTray => _systemTray!;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IConfiguration>(context.Configuration);
                services.AddSingleton<WidgetManagerService>();
                services.AddSingleton<SystemTrayService>();
                services.AddSingleton<SettingsService>();
                services.AddSingleton<WidgetPersistenceService>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<WidgetGalleryViewModel>();
                services.AddTransient<OverlayViewModel>();
                services.AddTransient<StickyNoteWidgetViewModel>();
                services.AddTransient<ClockWidgetViewModel>();
                services.AddTransient<WeatherWidgetViewModel>();
                services.AddTransient<SystemMonitorWidgetViewModel>();
                services.AddTransient<TodoListWidgetViewModel>();
                services.AddTransient<CalendarWidgetViewModel>();
                services.AddTransient<CalculatorWidgetViewModel>();
                services.AddTransient<ColorPickerWidgetViewModel>();
            })
            .Build();

        _widgetManager = Services.GetRequiredService<WidgetManagerService>();
        _systemTray = Services.GetRequiredService<SystemTrayService>();
        
        _widgetManager.Initialize();
        _systemTray.Initialize(this);

        _overlayWindow = new OverlayWindow();
        _overlayWindow.Activate();

        if (args.Arguments.Contains("--show-gallery"))
        {
            ShowWidgetGallery();
        }
        else if (args.Arguments.Contains("--settings"))
        {
            ShowSettings();
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Exception}");
    }

    public void ShowSettings()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        }
        _settingsWindow.Activate();
    }

    public void ShowWidgetGallery()
    {
        if (_galleryWindow == null)
        {
            _galleryWindow = new WidgetGalleryWindow();
            _galleryWindow.Closed += (s, e) => _galleryWindow = null;
        }
        _galleryWindow.Activate();
    }

    public void ToggleOverlay()
    {
        if (_overlayWindow != null)
        {
            if (_overlayWindow.Visible)
                _overlayWindow.Hide();
            else
                _overlayWindow.Show();
        }
    }

    protected override void OnExit()
    {
        _widgetManager?.SaveAllWidgets();
        _systemTray?.Dispose();
        _host?.Dispose();
        base.OnExit();
    }
}

public partial class App : Application
{
    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial BOOL SetProcessDpiAwarenessContext(IntPtr dpiContext);

    private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;
}