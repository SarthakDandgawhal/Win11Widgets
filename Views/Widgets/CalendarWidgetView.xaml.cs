using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class CalendarWidgetView : UserControl
{
    public CalendarWidgetView()
    {
        InitializeComponent();
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is CalendarWidgetViewModel vm)
        {
            var newWidth = Math.Max(280, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(280, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }
}