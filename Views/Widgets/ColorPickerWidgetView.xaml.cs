using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class ColorPickerWidgetView : UserControl
{
    public ColorPickerWidgetView()
    {
        InitializeComponent();
    }

    private void SavedColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is string color)
        {
            if (DataContext is ColorPickerWidgetViewModel vm)
            {
                vm.SelectSavedColorCommand.Execute(color);
            }
        }
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is ColorPickerWidgetViewModel vm)
        {
            var newWidth = Math.Max(250, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(280, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }
}