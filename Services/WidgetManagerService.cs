using System.Collections.Concurrent;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;
using Win11Widgets.Messages;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Services;

public partial class WidgetManagerService : ObservableObject
{
    private readonly ILogger<WidgetManagerService> _logger;
    private readonly WidgetPersistenceService _persistence;
    private readonly SettingsService _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, WidgetViewModelBase> _activeWidgets = new();
    private readonly List<WidgetDefinition> _widgetDefinitions = new();
    private int _nextZIndex = 1000;

    public IReadOnlyDictionary<string, WidgetViewModelBase> ActiveWidgets => _activeWidgets;
    public IReadOnlyList<WidgetDefinition> WidgetDefinitions => _widgetDefinitions;

    public WidgetManagerService(
        ILogger<WidgetManagerService> logger,
        WidgetPersistenceService persistence,
        SettingsService settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _persistence = persistence;
        _settings = settings;
        _serviceProvider = serviceProvider;
        RegisterBuiltInWidgets();
    }

    public void Initialize()
    {
        LoadWidgets();
        _logger.LogInformation("Widget manager initialized with {Count} widgets", _activeWidgets.Count);
    }

    private void RegisterBuiltInWidgets()
    {
        _widgetDefinitions.AddRange(new[]
        {
            new WidgetDefinition
            {
                Type = WidgetType.StickyNote,
                Name = "Sticky Note",
                Description = "A simple note widget with markdown support",
                IconGlyph = "\uE70F", // Document
                Category = "Productivity",
                DefaultWidth = 320,
                DefaultHeight = 240,
                ViewModelType = typeof(StickyNoteWidgetViewModel),
                ViewType = typeof(Views.Widgets.StickyNoteWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.Clock,
                Name = "Clock",
                Description = "Digital/analog clock with timezone support",
                IconGlyph = "\uE917", // Clock
                Category = "Utilities",
                DefaultWidth = 200,
                DefaultHeight = 200,
                ViewModelType = typeof(ClockWidgetViewModel),
                ViewType = typeof(Views.Widgets.ClockWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.Weather,
                Name = "Weather",
                Description = "Current weather and forecast",
                IconGlyph = "\uE708", // Cloud
                Category = "Information",
                DefaultWidth = 320,
                DefaultHeight = 280,
                ViewModelType = typeof(WeatherWidgetViewModel),
                ViewType = typeof(Views.Widgets.WeatherWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.SystemMonitor,
                Name = "System Monitor",
                Description = "CPU, Memory, Disk, Network usage graphs",
                IconGlyph = "\uEC4A", // Monitor
                Category = "System",
                DefaultWidth = 400,
                DefaultHeight = 300,
                ViewModelType = typeof(SystemMonitorWidgetViewModel),
                ViewType = typeof(Views.Widgets.SystemMonitorWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.TodoList,
                Name = "Todo List",
                Description = "Task management with priorities and due dates",
                IconGlyph = "\uE8EC", // Checkbox
                Category = "Productivity",
                DefaultWidth = 320,
                DefaultHeight = 400,
                ViewModelType = typeof(TodoListWidgetViewModel),
                ViewType = typeof(Views.Widgets.TodoListWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.Calendar,
                Name = "Calendar",
                Description = "Monthly calendar with events",
                IconGlyph = "\uE787", // Calendar
                Category = "Productivity",
                DefaultWidth = 320,
                DefaultHeight = 320,
                ViewModelType = typeof(CalendarWidgetViewModel),
                ViewType = typeof(Views.Widgets.CalendarWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.Calculator,
                Name = "Calculator",
                Description = "Quick calculator widget",
                IconGlyph = "\uEC2A", // Calculator
                Category = "Utilities",
                DefaultWidth = 280,
                DefaultHeight = 360,
                ViewModelType = typeof(CalculatorWidgetViewModel),
                ViewType = typeof(Views.Widgets.CalculatorWidgetView)
            },
            new WidgetDefinition
            {
                Type = WidgetType.ColorPicker,
                Name = "Color Picker",
                Description = "Pick and save colors with multiple formats",
                IconGlyph = "\uE790", // Color
                Category = "Design",
                DefaultWidth = 280,
                DefaultHeight = 320,
                ViewModelType = typeof(ColorPickerWidgetViewModel),
                ViewType = typeof(Views.Widgets.ColorPickerWidgetView)
            }
        });
    }

    public WidgetViewModelBase? CreateWidget(WidgetType type, double x = 100, double y = 100)
    {
        var definition = _widgetDefinitions.FirstOrDefault(d => d.Type == type);
        if (definition == null)
        {
            _logger.LogWarning("Widget type {Type} not registered", type);
            return null;
        }

        var state = CreateWidgetState(type);
        state.X = x;
        state.Y = y;
        state.Width = definition.DefaultWidth;
        state.Height = definition.DefaultHeight;
        state.Title = definition.Name;
        state.ZIndex = _nextZIndex++;

        var viewModel = CreateViewModel(definition, state);
        viewModel.Initialize(state);

        if (_activeWidgets.TryAdd(state.Id, viewModel))
        {
            _persistence.SaveWidgetState(state);
            WeakReferenceMessenger.Default.Send(new WidgetCreatedMessage(viewModel));
            _logger.LogInformation("Created widget {Type} with ID {Id}", type, state.Id);
            return viewModel;
        }

        return null;
    }

    private WidgetState CreateWidgetState(WidgetType type) => type switch
    {
        WidgetType.StickyNote => new StickyNoteWidgetState(),
        WidgetType.Clock => new ClockWidgetState(),
        WidgetType.Weather => new WeatherWidgetState(),
        WidgetType.SystemMonitor => new SystemMonitorWidgetState(),
        WidgetType.TodoList => new TodoListWidgetState(),
        WidgetType.Calendar => new CalendarWidgetState(),
        WidgetType.Calculator => new CalculatorWidgetState(),
        WidgetType.ColorPicker => new ColorPickerWidgetState(),
        _ => new StickyNoteWidgetState()
    };

    private WidgetViewModelBase CreateViewModel(WidgetDefinition definition, WidgetState state)
    {
        var viewModel = _serviceProvider.GetRequiredService(definition.ViewModelType!) as WidgetViewModelBase;
        return viewModel!;
    }

    public bool RemoveWidget(string widgetId)
    {
        if (_activeWidgets.TryRemove(widgetId, out var viewModel))
        {
            viewModel.Cleanup();
            _persistence.DeleteWidgetState(widgetId);
            WeakReferenceMessenger.Default.Send(new WidgetRemovedMessage(widgetId));
            _logger.LogInformation("Removed widget {Id}", widgetId);
            return true;
        }
        return false;
    }

    public WidgetViewModelBase? GetWidget(string widgetId)
    {
        _activeWidgets.TryGetValue(widgetId, out var widget);
        return widget;
    }

    public void BringToFront(string widgetId)
    {
        if (_activeWidgets.TryGetValue(widgetId, out var widget))
        {
            widget.State.ZIndex = _nextZIndex++;
            OnPropertyChanged(nameof(ActiveWidgets));
        }
    }

    public void UpdateWidgetPosition(string widgetId, double x, double y)
    {
        if (_activeWidgets.TryGetValue(widgetId, out var widget))
        {
            widget.State.X = x;
            widget.State.Y = y;
            widget.State.ModifiedAt = DateTime.Now;
            _persistence.SaveWidgetState(widget.State);
        }
    }

    public void UpdateWidgetSize(string widgetId, double width, double height)
    {
        if (_activeWidgets.TryGetValue(widgetId, out var widget))
        {
            widget.State.Width = width;
            widget.State.Height = height;
            widget.State.ModifiedAt = DateTime.Now;
            _persistence.SaveWidgetState(widget.State);
        }
    }

    public void SaveAllWidgets()
    {
        foreach (var widget in _activeWidgets.Values)
        {
            _persistence.SaveWidgetState(widget.State);
        }
        _logger.LogInformation("Saved all {Count} widgets", _activeWidgets.Count);
    }

    private void LoadWidgets()
    {
        var states = _persistence.LoadAllWidgetStates();
        foreach (var state in states)
        {
            var definition = _widgetDefinitions.FirstOrDefault(d => d.Type == state.Type);
            if (definition != null)
            {
                var viewModel = CreateViewModel(definition, state);
                viewModel.Initialize(state);
                _activeWidgets.TryAdd(state.Id, viewModel);
                _nextZIndex = Math.Max(_nextZIndex, state.ZIndex + 1);
            }
        }
    }

    public WidgetDefinition? GetDefinition(WidgetType type)
    {
        return _widgetDefinitions.FirstOrDefault(d => d.Type == type);
    }
}