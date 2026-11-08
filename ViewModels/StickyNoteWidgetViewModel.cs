using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class StickyNoteWidgetViewModel : WidgetViewModelBase
{
    private static readonly string[] NoteColors = new[]
    {
        "#FFF9C4", // Yellow
        "#FCE4EC", // Pink
        "#E3F2FD", // Blue
        "#E8F5E9", // Green
        "#FFF3E0", // Orange
        "#F3E5F5", // Purple
        "#E0F2F1", // Teal
        "#FBE9E7", // Deep Orange
        "#FFFFFF"  // White
    };

    [ObservableProperty]
    private string _content = "";

    [ObservableProperty]
    private string _selectedColor = "#FFF9C4";

    [ObservableProperty]
    private bool _isEditing = false;

    public override WidgetType WidgetType => WidgetType.StickyNote;
    public IReadOnlyList<string> AvailableColors => NoteColors;

    public StickyNoteWidgetViewModel(
        ILogger<StickyNoteWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override void LoadState(WidgetState state)
    {
        if (state is StickyNoteWidgetState noteState)
        {
            Content = noteState.Content;
            SelectedColor = noteState.Color;
            State.Title = string.IsNullOrWhiteSpace(noteState.Content) ? "Sticky Note" : "Sticky Note";
        }
    }

    [RelayCommand]
    public void StartEdit()
    {
        IsEditing = true;
    }

    [RelayCommand]
    public void StopEdit()
    {
        IsEditing = false;
        if (State is StickyNoteWidgetState noteState)
        {
            noteState.Content = Content;
            noteState.Color = SelectedColor;
        }
        SaveState();
    }

    [RelayCommand]
    public void SetColor(string color)
    {
        SelectedColor = color;
        if (State is StickyNoteWidgetState noteState)
        {
            noteState.Color = color;
        }
        SaveState();
    }

    partial void OnContentChanged(string value)
    {
        if (!IsEditing && State is StickyNoteWidgetState noteState)
        {
            noteState.Content = value;
            State.Title = string.IsNullOrWhiteSpace(value) ? "Sticky Note" : 
                value.Length > 30 ? value.Substring(0, 30) + "..." : value;
        }
    }

    protected override void CopyStateTo(WidgetViewModelBase target)
    {
        if (target is StickyNoteWidgetViewModel noteTarget && State is StickyNoteWidgetState sourceState)
        {
            if (target.State is StickyNoteWidgetState targetState)
            {
                targetState.Content = sourceState.Content;
                targetState.Color = sourceState.Color;
                targetState.FontFamily = sourceState.FontFamily;
                targetState.FontSize = sourceState.FontSize;
                target.Content = sourceState.Content;
                target.SelectedColor = sourceState.Color;
            }
        }
    }
}