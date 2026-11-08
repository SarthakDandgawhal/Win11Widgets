using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views.Widgets;

public sealed partial class SystemMonitorWidgetView : UserControl
{
    public SystemMonitorWidgetView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateGraphs();
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (args.NewValue is SystemMonitorWidgetViewModel vm)
        {
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(vm.CpuHistory) ||
                    e.PropertyName == nameof(vm.MemoryHistory) ||
                    e.PropertyName == nameof(vm.DiskHistory) ||
                    e.PropertyName == nameof(vm.NetworkHistory))
                {
                    UpdateGraphs();
                }
            };
        }
    }

    private void UpdateGraphs()
    {
        if (DataContext is not SystemMonitorWidgetViewModel vm) return;

        DrawGraph(CpuGraph, vm.CpuHistory, Microsoft.UI.Colors.IndianRed);
        DrawGraph(MemoryGraph, vm.MemoryHistory, Microsoft.UI.Colors.MediumTurquoise);
        DrawGraph(DiskGraph, vm.DiskHistory, Microsoft.UI.Colors.Gold);
        DrawGraph(NetworkGraph, vm.NetworkHistory, Microsoft.UI.Colors.MediumSeaGreen);
    }

    private void DrawGraph(Canvas canvas, System.Collections.Generic.IEnumerable<double> history, Microsoft.UI.Color color)
    {
        canvas.Children.Clear();
        
        var values = new List<double>(history);
        if (values.Count < 2) return;

        var width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 300;
        var height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 40;
        var stepX = width / (values.Count - 1);

        var geometry = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = new Point(0, height - (values[0] / 100.0 * height)),
            IsClosed = false
        };

        for (int i = 1; i < values.Count; i++)
        {
            var x = i * stepX;
            var y = height - (values[i] / 100.0 * height);
            figure.Segments.Add(new LineSegment { Point = new Point(x, y) });
        }

        geometry.Figures.Add(figure);

        var path = new Path
        {
            Data = geometry,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            Fill = null
        };

        // Add gradient fill
        var fillGeometry = geometry.Clone();
        var fillFigure = new PathFigure
        {
            StartPoint = new Point(0, height),
            IsClosed = true
        };
        fillFigure.Segments.Add(new LineSegment { Point = new Point(0, height - (values[0] / 100.0 * height)) });
        
        for (int i = 1; i < values.Count; i++)
        {
            var x = i * stepX;
            var y = height - (values[i] / 100.0 * height);
            fillFigure.Segments.Add(new LineSegment { Point = new Point(x, y) });
        }
        
        fillFigure.Segments.Add(new LineSegment { Point = new Point((values.Count - 1) * stepX, height) });
        fillGeometry.Figures.Add(fillFigure);

        var fillPath = new Path
        {
            Data = fillGeometry,
            Fill = new SolidColorBrush(color) { Opacity = 0.15 },
            Stroke = null
        };

        canvas.Children.Add(fillPath);
        canvas.Children.Add(path);
    }

    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (DataContext is SystemMonitorWidgetViewModel vm)
        {
            var newWidth = Math.Max(350, vm.State.Width + e.HorizontalChange);
            var newHeight = Math.Max(250, vm.State.Height + e.VerticalChange);
            vm.Resize(newWidth, newHeight);
        }
    }
}