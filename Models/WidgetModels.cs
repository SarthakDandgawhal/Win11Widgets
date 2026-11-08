using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Win11Widgets.Models;

public enum WidgetType
{
    StickyNote,
    Clock,
    Weather,
    SystemMonitor,
    TodoList,
    Calendar,
    Calculator,
    ColorPicker,
    Custom
}

public enum WidgetDockPosition
{
    None,
    TopLeft,
    TopCenter,
    TopRight,
    CenterLeft,
    Center,
    CenterRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(StickyNoteWidgetState), nameof(WidgetType.StickyNote))]
[JsonDerivedType(typeof(ClockWidgetState), nameof(WidgetType.Clock))]
[JsonDerivedType(typeof(WeatherWidgetState), nameof(WidgetType.Weather))]
[JsonDerivedType(typeof(SystemMonitorWidgetState), nameof(WidgetType.SystemMonitor))]
[JsonDerivedType(typeof(TodoListWidgetState), nameof(WidgetType.TodoList))]
[JsonDerivedType(typeof(CalendarWidgetState), nameof(WidgetType.Calendar))]
[JsonDerivedType(typeof(CalculatorWidgetState), nameof(WidgetType.Calculator))]
[JsonDerivedType(typeof(ColorPickerWidgetState), nameof(WidgetType.ColorPicker))]
public abstract partial class WidgetState : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public abstract WidgetType Type { get; }
    public string Title { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 320;
    public double Height { get; set; } = 240;
    public double Opacity { get; set; } = 0.95;
    public bool IsPinned { get; set; } = false;
    public bool IsVisible { get; set; } = true;
    public int ZIndex { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public WidgetDockPosition DockPosition { get; set; } = WidgetDockPosition.None;
    public bool SnapToGrid { get; set; } = true;
}

public partial class StickyNoteWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.StickyNote;
    public string Content { get; set; } = "";
    public string Color { get; set; } = "#FFF9C4"; // Yellow
    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 13;
    public bool ShowTimestamp { get; set; } = true;
}

public partial class ClockWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.Clock;
    public bool ShowSeconds { get; set; } = false;
    public bool Use24HourFormat { get; set; } = true;
    public bool ShowDate { get; set; } = true;
    public string TimeZone { get; set; } = "Local";
    public ClockStyle Style { get; set; } = ClockStyle.Digital;
}

public enum ClockStyle
{
    Digital,
    Analog,
    Binary,
    Text
}

public partial class WeatherWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.Weather;
    public string Location { get; set; } = "Auto";
    public string ApiKey { get; set; } = "";
    public TemperatureUnit Unit { get; set; } = TemperatureUnit.Celsius;
    public int RefreshIntervalMinutes { get; set; } = 30;
    public bool ShowForecast { get; set; } = true;
    public int ForecastDays { get; set; } = 3;
}

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit,
    Kelvin
}

public partial class SystemMonitorWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.SystemMonitor;
    public bool ShowCpu { get; set; } = true;
    public bool ShowMemory { get; set; } = true;
    public bool ShowDisk { get; set; } = true;
    public bool ShowNetwork { get; set; } = true;
    public bool ShowGpu { get; set; } = false;
    public int RefreshIntervalMs { get; set; } = 1000;
    public int HistoryPoints { get; set; } = 60;
    public GraphStyle GraphStyle { get; set; } = GraphStyle.Line;
}

public enum GraphStyle
{
    Line,
    Bar,
    Dot,
    Area
}

public partial class TodoListWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.TodoList;
    public List<TodoItem> Items { get; set; } = new();
    public bool ShowCompleted { get; set; } = false;
    public bool AutoSort { get; set; } = true;
    public SortBy SortBy { get; set; } = SortBy.CreatedDate;
}

public enum SortBy
{
    CreatedDate,
    DueDate,
    Priority,
    Alphabetical
}

public partial class TodoItem : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int Order { get; set; } = 0;
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public partial class CalendarWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.Calendar;
    public bool ShowWeekNumbers { get; set; } = false;
    public int FirstDayOfWeek { get; set; } = 0; // 0 = Sunday
    public bool ShowEvents { get; set; } = true;
    public string CalendarSource { get; set; } = "System";
}

public partial class CalculatorWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.Calculator;
    public CalculatorMode Mode { get; set; } = CalculatorMode.Standard;
    public int DecimalPlaces { get; set; } = 2;
    public bool ShowHistory { get; set; } = true;
}

public enum CalculatorMode
{
    Standard,
    Scientific,
    Programmer,
    DateCalculation
}

public partial class ColorPickerWidgetState : WidgetState
{
    public override WidgetType Type => WidgetType.ColorPicker;
    public string SelectedColor { get; set; } = "#0078D4";
    public ColorFormat DefaultFormat { get; set; } = ColorFormat.Hex;
    public bool ShowPalette { get; set; } = true;
    public List<string> SavedColors { get; set; } = new();
}

public enum ColorFormat
{
    Hex,
    Rgb,
    Hsl,
    Cmyk
}

public class WidgetDefinition
{
    public WidgetType Type { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string IconGlyph { get; set; } = "\uE70F"; // Default document icon
    public string Category { get; set; } = "General";
    public double DefaultWidth { get; set; } = 320;
    public double DefaultHeight { get; set; } = 240;
    public bool Resizable { get; set; } = true;
    public bool SupportsTransparency { get; set; } = true;
    public bool SupportsDocking { get; set; } = true;
    public Type StateType { get; set; } = typeof(WidgetState);
    public Type ViewModelType { get; set; }
    public Type ViewType { get; set; }
}