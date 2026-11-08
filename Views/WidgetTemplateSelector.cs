using Microsoft.UI.Xaml.Controls;
using Win11Widgets.ViewModels;

namespace Win11Widgets.Views;

public class WidgetTemplateSelector : DataTemplateSelector
{
    public DataTemplate StickyNoteTemplate { get; set; } = null!;
    public DataTemplate ClockTemplate { get; set; } = null!;
    public DataTemplate WeatherTemplate { get; set; } = null!;
    public DataTemplate SystemMonitorTemplate { get; set; } = null!;
    public DataTemplate TodoListTemplate { get; set; } = null!;
    public DataTemplate CalendarTemplate { get; set; } = null!;
    public DataTemplate CalculatorTemplate { get; set; } = null!;
    public DataTemplate ColorPickerTemplate { get; set; } = null!;

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return item switch
        {
            StickyNoteWidgetViewModel => StickyNoteTemplate,
            ClockWidgetViewModel => ClockTemplate,
            WeatherWidgetViewModel => WeatherTemplate,
            SystemMonitorWidgetViewModel => SystemMonitorTemplate,
            TodoListWidgetViewModel => TodoListTemplate,
            CalendarWidgetViewModel => CalendarTemplate,
            CalculatorWidgetViewModel => CalculatorTemplate,
            ColorPickerWidgetViewModel => ColorPickerTemplate,
            _ => base.SelectTemplateCore(item)
        };
    }
}