using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class WeatherWidgetView : UserControl
{
    public WeatherWidgetView()
    {
        InitializeComponent();
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is WeatherWidgetViewModel vm)
        {
            var newWidth = Math.Max(280, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(200, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }
}