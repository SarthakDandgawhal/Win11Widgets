using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class TodoListWidgetViewModel : WidgetViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<TodoItemViewModel> _items = new();

    [ObservableProperty]
    private string _newItemText = "";

    [ObservableProperty]
    private bool _showCompleted = false;

    public override WidgetType WidgetType => WidgetType.TodoList;

    public TodoListWidgetViewModel(
        ILogger<TodoListWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override void LoadState(WidgetState state)
    {
        if (state is TodoListWidgetState todoState)
        {
            ShowCompleted = todoState.ShowCompleted;
            Items.Clear();
            foreach (var item in todoState.Items.OrderBy(i => i.Order))
            {
                Items.Add(new TodoItemViewModel(item, this));
            }
        }
    }

    [RelayCommand]
    public void AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemText)) return;

        var item = new TodoItem
        {
            Text = NewItemText.Trim(),
            CreatedAt = DateTime.Now,
            Order = Items.Count
        };

        if (State is TodoListWidgetState todoState)
        {
            todoState.Items.Add(item);
        }

        Items.Add(new TodoItemViewModel(item, this));
        NewItemText = "";
        SaveState();
    }

    [RelayCommand]
    public void RemoveItem(TodoItemViewModel item)
    {
        Items.Remove(item);
        if (State is TodoListWidgetState todoState)
        {
            todoState.Items.Remove(item.Model);
        }
        SaveState();
    }

    [RelayCommand]
    public void ToggleComplete(TodoItemViewModel item)
    {
        item.Model.IsCompleted = !item.Model.IsCompleted;
        SaveState();
    }

    [RelayCommand]
    public void ToggleShowCompleted()
    {
        ShowCompleted = !ShowCompleted;
        if (State is TodoListWidgetState todoState)
        {
            todoState.ShowCompleted = ShowCompleted;
        }
        SaveState();
    }

    [RelayCommand]
    public void ReorderItems()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            Items[i].Model.Order = i;
        }
        SaveState();
    }

    public IEnumerable<TodoItemViewModel> VisibleItems =>
        ShowCompleted ? Items : Items.Where(i => !i.Model.IsCompleted);
}

public partial class TodoItemViewModel : ObservableObject
{
    public TodoItem Model { get; }

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private string _editText = "";

    public TodoItemViewModel(TodoItem model, TodoListWidgetViewModel parent)
    {
        Model = model;
        EditText = model.Text;
    }

    public string Text => Model.Text;
    public bool IsCompleted => Model.IsCompleted;
    public Priority Priority => Model.Priority;
    public DateTime? DueDate => Model.DueDate;

    public void StartEdit()
    {
        EditText = Model.Text;
        IsEditing = true;
    }

    public void SaveEdit()
    {
        if (!string.IsNullOrWhiteSpace(EditText))
        {
            Model.Text = EditText.Trim();
            IsEditing = false;
        }
    }

    public void CancelEdit()
    {
        EditText = Model.Text;
        IsEditing = false;
    }
}