using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class StickyNoteWidgetView : UserControl
{
    private double _startWidth;
    private double _startHeight;

    public StickyNoteWidgetView()
    {
        InitializeComponent();
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is StickyNoteWidgetViewModel vm)
        {
            var newWidth = Math.Max(200, _startWidth + e.HorizontalChange);
            var newHeight = Math.Max(150, _startHeight + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }

    private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        // Save handled in ViewModel
    }

    private void MoreColors_Click(object sender, RoutedEventArgs e)
    {
        // Show color picker flyout
        var flyout = new MenuFlyout();
        var colors = new[] 
        { 
            "#FFF9C4", "#FCE4EC", "#E3F2FD", "#E8F5E9", 
            "#FFF3E0", "#F3E5F5", "#E0F2F1", "#FBE9E7",
            "#FFFFFF", "#FFEBEE", "#E8EAF6", "#E0F7FA",
            "#FFFDE7", "#F1F8E9", "#FCE4EC", "#ECEFF1"
        };

        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Margin = 8 };
        foreach (var color in colors)
        {
            var btn = new Button
            {
                Width = 28,
                Height = 28,
                Padding = new Thickness(0),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255,
                        byte.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                        byte.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber))),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Tag = color
            };
            btn.Click += (s, args) => 
            {
                if (DataContext is StickyNoteWidgetViewModel vm && s is Button b)
                {
                    vm.SetColorCommand.Execute(b.Tag);
                }
                flyout.Hide();
            };
            panel.Children.Add(btn);
        }

        flyout.Content = panel;
        flyout.ShowAt(sender as FrameworkElement);
    }

    protected override void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        base.OnDataContextChanged(sender, args);
        if (args.NewValue is StickyNoteWidgetViewModel vm)
        {
            _startWidth = vm.State.Width;
            _startHeight = vm.State.Height;
        }
    }
}