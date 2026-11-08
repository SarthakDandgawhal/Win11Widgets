using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;
using Win11Widgets.Messages;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public abstract partial class WidgetViewModelBase : ObservableObject, IDisposable
{
    protected readonly ILogger _logger;
    protected readonly WidgetManagerService _widgetManager;
    protected readonly WidgetPersistenceService _persistence;
    private bool _isDisposed = false;

    [ObservableProperty]
    private WidgetState _state;

    [ObservableProperty]
    private bool _isSelected = false;

    [ObservableProperty]
    private bool _isDragging = false;

    [ObservableProperty]
    private double _dragOffsetX;

    [ObservableProperty]
    private double _dragOffsetY;

    protected WidgetViewModelBase(
        ILogger logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
    {
        _logger = logger;
        _widgetManager = widgetManager;
        _persistence = persistence;
        _settings = settings;
    }

    protected readonly SettingsService _settings;

    public WidgetState State { get; private set; } = null!;

    public virtual void Initialize(WidgetState state)
    {
        State = state;
        OnPropertyChanged(nameof(State));
        LoadState(state);
    }

    protected virtual void LoadState(WidgetState state) { }

    public virtual void SaveState()
    {
        State.ModifiedAt = DateTime.Now;
        _persistence.SaveWidgetState(State);
        WeakReferenceMessenger.Default.Send(new WidgetUpdatedMessage(this));
    }

    [RelayCommand]
    public virtual void StartDrag(double offsetX, double offsetY)
    {
        IsDragging = true;
        DragOffsetX = offsetX;
        DragOffsetY = offsetY;
        _widgetManager.BringToFront(State.Id);
    }

    [RelayCommand]
    public virtual void Drag(double screenX, double screenY)
    {
        if (!IsDragging) return;

        var newX = screenX - DragOffsetX;
        var newY = screenY - DragOffsetY;

        if (State.SnapToGrid)
        {
            var gridSize = _settings.Settings.GridSize;
            newX = Math.Round(newX / gridSize) * gridSize;
            newY = Math.Round(newY / gridSize) * gridSize;
        }

        State.X = Math.Max(0, newX);
        State.Y = Math.Max(0, newY);
        OnPropertyChanged(nameof(State));
    }

    [RelayCommand]
    public virtual void EndDrag()
    {
        IsDragging = false;
        _widgetManager.UpdateWidgetPosition(State.Id, State.X, State.Y);
        SaveState();
    }

    [RelayCommand]
    public virtual void Resize(double width, double height)
    {
        State.Width = Math.Max(150, width);
        State.Height = Math.Max(100, height);
        _widgetManager.UpdateWidgetSize(State.Id, State.Width, State.Height);
        OnPropertyChanged(nameof(State));
        SaveState();
    }

    [RelayCommand]
    public virtual void Pin()
    {
        State.IsPinned = !State.IsPinned;
        SaveState();
    }

    [RelayCommand]
    public virtual void Close()
    {
        _widgetManager.RemoveWidget(State.Id);
    }

    [RelayCommand]
    public virtual void Duplicate()
    {
        var newWidget = _widgetManager.CreateWidget(State.Type, State.X + 30, State.Y + 30);
        if (newWidget != null)
        {
            // Copy relevant state properties
            CopyStateTo(newWidget);
        }
    }

    protected virtual void CopyStateTo(WidgetViewModelBase target) { }

    [RelayCommand]
    public virtual void BringToFront()
    {
        _widgetManager.BringToFront(State.Id);
    }

    [RelayCommand]
    public virtual void SendToBack()
    {
        State.ZIndex = 0;
        SaveState();
    }

    public virtual void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            Cleanup();
        }
    }

    protected virtual void Cleanup() { }

    public abstract WidgetType WidgetType { get; }
}