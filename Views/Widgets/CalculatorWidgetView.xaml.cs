using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class CalculatorWidgetView : UserControl
{
    public CalculatorWidgetView()
    {
        InitializeComponent();
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is CalculatorWidgetViewModel vm)
        {
            var newWidth = Math.Max(250, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(300, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }

    private void History_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is CalculatorWidgetViewModel vm)
        {
            var flyout = new MenuFlyout();
            
            if (vm.History.Count == 0)
            {
                flyout.Items.Add(new MenuFlyoutItem { Text = "No history", IsEnabled = false });
            }
            else
            {
                foreach (var entry in vm.History)
                {
                    var item = new MenuFlyoutItem { Text = entry };
                    item.Click += (s, args) => 
                    {
                        // Could parse and use the result
                    };
                    flyout.Items.Add(item);
                }
                
                flyout.Items.Add(new MenuFlyoutSeparator());
                var clearItem = new MenuFlyoutItem { Text = "Clear History" };
                clearItem.Click += (s, args) => vm.History.Clear();
                flyout.Items.Add(clearItem);
            }

            flyout.ShowAt(sender as FrameworkElement);
        }
    }
}