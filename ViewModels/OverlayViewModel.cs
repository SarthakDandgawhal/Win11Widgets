using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Win11Widgets.Messages;
using Win11Widgets.Models;
using Win11Widgets.Services;
using Win11Widgets.ViewModels;

namespace Win11Widgets.ViewModels;

public partial class OverlayViewModel : ObservableObject, IRecipient<WidgetCreatedMessage>, IRecipient<WidgetRemovedMessage>
{
    private readonly WidgetManagerService _widgetManager;
    private readonly ILogger<OverlayViewModel> _logger;

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private string _widgetCountText = "0 widgets";

    public OverlayViewModel(WidgetManagerService widgetManager, ILogger<OverlayViewModel> logger)
    {
        _widgetManager = widgetManager;
        _logger = logger;
        
        WeakReferenceMessenger.Default.RegisterAll(this);
        UpdateWidgetCount();
    }

    public IReadOnlyDictionary<string, WidgetViewModelBase> Widgets => _widgetManager.ActiveWidgets;

    public void Receive(WidgetCreatedMessage message)
    {
        UpdateWidgetCount();
    }

    public void Receive(WidgetRemovedMessage message)
    {
        UpdateWidgetCount();
    }

    private void UpdateWidgetCount()
    {
        var count = _widgetManager.ActiveWidgets.Count;
        WidgetCountText = count == 1 ? "1 widget" : $"{count} widgets";
    }

    [RelayCommand]
    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
        WeakReferenceMessenger.Default.Send(new OverlayVisibilityChangedMessage(IsVisible));
    }

    [RelayCommand]
    public void ShowGallery()
    {
        if (App.Current is App app)
        {
            app.ShowWidgetGallery();
        }
    }

    [RelayCommand]
    public void ShowSettings()
    {
        if (App.Current is App app)
        {
            app.ShowSettings();
        }
    }

    [RelayCommand]
    public void CreateWidget(WidgetType type)
    {
        _widgetManager.CreateWidget(type);
    }
}