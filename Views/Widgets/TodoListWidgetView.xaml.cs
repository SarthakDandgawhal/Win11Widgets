using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class TodoListWidgetView : UserControl
{
    public TodoListWidgetView()
    {
        InitializeComponent();
    }

    private void NewItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (DataContext is TodoListWidgetViewModel vm)
            {
                vm.AddItemCommand.Execute(null);
            }
        }
    }

    private void EditTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (sender is TextBox tb && tb.DataContext is TodoItemViewModel item)
            {
                item.SaveEdit();
            }
        }
        else if (e.Key == Windows.System.VirtualKey.Escape)
        {
            if (sender is TextBox tb && tb.DataContext is TodoItemViewModel item)
            {
                item.CancelEdit();
            }
        }
    }

    private void EditTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.DataContext is TodoItemViewModel item)
        {
            item.SaveEdit();
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
        {
            item.StartEdit();
        }
    }

    private void SaveEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
        {
            item.SaveEdit();
        }
    }

    private void CancelEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
        {
            item.CancelEdit();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is TodoItemViewModel item)
        {
            if (DataContext is TodoListWidgetViewModel vm)
            {
                vm.RemoveItemCommand.Execute(item);
            }
        }
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is TodoListWidgetViewModel vm)
        {
            var newWidth = Math.Max(280, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(200, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }
}