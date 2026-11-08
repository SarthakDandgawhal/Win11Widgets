using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class ClockWidgetView : UserControl
{
    public ClockWidgetView()
    {
        InitializeComponent();
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is ClockWidgetViewModel vm)
        {
            var newWidth = Math.Max(150, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(150, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // Show settings flyout
        var flyout = new MenuFlyout();
        
        var formats = new[] { ("24-hour", true), ("12-hour", false) };
        foreach (var (name, is24) in formats)
        {
            var item = new MenuFlyoutItem { Text = name };
            item.Click += (s, args) => 
            {
                if (DataContext is ClockWidgetViewModel vm && vm.State is Win11Widgets.Models.ClockWidgetState state)
                {
                    state.Use24HourFormat = is24;
                    vm.SaveState();
                }
            };
            flyout.Items.Add(item);
        }

        flyout.Items.Add(new MenuFlyoutSeparator());
        
        var showSecondsItem = new MenuFlyoutItem { Text = "Show Seconds" };
        showSecondsItem.Click += (s, args) => 
        {
            if (DataContext is ClockWidgetViewModel vm && vm.State is Win11Widgets.Models.ClockWidgetState state)
            {
                state.ShowSeconds = !state.ShowSeconds;
                vm.SaveState();
            }
        };
        flyout.Items.Add(showSecondsItem);

        var showDateItem = new MenuFlyoutItem { Text = "Show Date" };
        showDateItem.Click += (s, args) => 
        {
            if (DataContext is ClockWidgetViewModel vm && vm.State is Win11Widgets.Models.ClockWidgetState state)
            {
                state.ShowDate = !state.ShowDate;
                vm.SaveState();
            }
        };
        flyout.Items.Add(showDateItem);

        flyout.ShowAt(sender as FrameworkElement);
    }
}