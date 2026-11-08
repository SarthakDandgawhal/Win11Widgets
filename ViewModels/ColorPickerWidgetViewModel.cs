using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class ColorPickerWidgetViewModel : WidgetViewModelBase
{
    [ObservableProperty]
    private string _selectedColor = "#0078D4";

    [ObservableProperty]
    private byte _red = 0;

    [ObservableProperty]
    private byte _green = 120;

    [ObservableProperty]
    private byte _blue = 212;

    [ObservableProperty]
    private double _hue = 205;

    [ObservableProperty]
    private double _saturation = 100;

    [ObservableProperty]
    private double _lightness = 41;

    [ObservableProperty]
    private ObservableCollection<string> _savedColors = new();

    [ObservableProperty]
    private ColorFormat _currentFormat = ColorFormat.Hex;

    public override WidgetType WidgetType => WidgetType.ColorPicker;

    public IEnumerable<string> FormatOptions => Enum.GetNames<ColorFormat>();

    public ColorPickerWidgetViewModel(
        ILogger<ColorPickerWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
        UpdateFromHex(SelectedColor);
    }

    protected override void LoadState(WidgetState state)
    {
        if (state is ColorPickerWidgetState colorState)
        {
            SelectedColor = colorState.SelectedColor;
            CurrentFormat = colorState.DefaultFormat;
            foreach (var color in colorState.SavedColors)
            {
                SavedColors.Add(color);
            }
            UpdateFromHex(SelectedColor);
        }
    }

    partial void OnSelectedColorChanged(string value)
    {
        if (!value.StartsWith("#")) return;
        UpdateFromHex(value);
        
        if (State is ColorPickerWidgetState colorState)
        {
            colorState.SelectedColor = value;
        }
        SaveState();
    }

    partial void OnRedChanged(byte value) => UpdateFromRgb();
    partial void OnGreenChanged(byte value) => UpdateFromRgb();
    partial void OnBlueChanged(byte value) => UpdateFromRgb();

    partial void OnHueChanged(double value) => UpdateFromHsl();
    partial void OnSaturationChanged(double value) => UpdateFromHsl();
    partial void OnLightnessChanged(double value) => UpdateFromHsl();

    private void UpdateFromHex(string hex)
    {
        if (hex.Length != 7) return;
        
        try
        {
            Red = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            Green = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            Blue = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            UpdateHslFromRgb();
        }
        catch { }
    }

    private void UpdateFromRgb()
    {
        SelectedColor = $"#{Red:X2}{Green:X2}{Blue:X2}";
        UpdateHslFromRgb();
    }

    private void UpdateFromHsl()
    {
        var (r, g, b) = HslToRgb(Hue, Saturation, Lightness);
        Red = r;
        Green = g;
        Blue = b;
        SelectedColor = $"#{Red:X2}{Green:X2}{Blue:X2}";
    }

    private void UpdateHslFromRgb()
    {
        var (h, s, l) = RgbToHsl(Red, Green, Blue);
        Hue = h;
        Saturation = s;
        Lightness = l;
    }

    [RelayCommand]
    public void SaveColor()
    {
        if (!SavedColors.Contains(SelectedColor))
        {
            SavedColors.Insert(0, SelectedColor);
            if (SavedColors.Count > 20) SavedColors.RemoveAt(SavedColors.Count - 1);
            
            if (State is ColorPickerWidgetState colorState)
            {
                colorState.SavedColors = SavedColors.ToList();
            }
            SaveState();
        }
    }

    [RelayCommand]
    public void RemoveColor(string color)
    {
        SavedColors.Remove(color);
        if (State is ColorPickerWidgetState colorState)
        {
            colorState.SavedColors = SavedColors.ToList();
        }
        SaveState();
    }

    [RelayCommand]
    public void SelectSavedColor(string color)
    {
        SelectedColor = color;
    }

    [RelayCommand]
    public void CopyColor()
    {
        var format = CurrentFormat switch
        {
            ColorFormat.Hex => SelectedColor,
            ColorFormat.Rgb => $"rgb({Red}, {Green}, {Blue})",
            ColorFormat.Hsl => $"hsl({Hue:F0}, {Saturation:F0}%, {Lightness:F0}%)",
            ColorFormat.Cmyk => RgbToCmyk(Red, Green, Blue),
            _ => SelectedColor
        };
        
        // Copy to clipboard - would need Windows.Data.Html.HtmlClipboard or similar
        _logger.LogInformation("Color copied: {Format}", format);
    }

    private static (double h, double s, double l) RgbToHsl(byte r, byte g, byte b)
    {
        double rn = r / 255.0, gn = g / 255.0, bn = b / 255.0;
        double max = Math.Max(rn, Math.Max(gn, bn));
        double min = Math.Min(rn, Math.Min(gn, bn));
        double h = 0, s = 0, l = (max + min) / 2;

        if (max != min)
        {
            double d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == rn) h = (gn - bn) / d + (gn < bn ? 6 : 0);
            else if (max == gn) h = (bn - rn) / d + 2;
            else h = (rn - gn) / d + 4;
            h *= 60;
        }
        return (h, s * 100, l * 100);
    }

    private static (byte r, byte g, byte b) HslToRgb(double h, double s, double l)
    {
        s /= 100; l /= 100;
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;
        double r = 0, g = 0, b = 0;

        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return (
            (byte)Math.Round((r + m) * 255),
            (byte)Math.Round((g + m) * 255),
            (byte)Math.Round((b + m) * 255)
        );
    }

    private static string RgbToCmyk(byte r, byte g, byte b)
    {
        double rn = r / 255.0, gn = g / 255.0, bn = b / 255.0;
        double k = 1 - Math.Max(rn, Math.Max(gn, bn));
        if (k == 1) return "cmyk(0%, 0%, 0%, 100%)";
        double c = (1 - rn - k) / (1 - k);
        double m = (1 - gn - k) / (1 - k);
        double y = (1 - bn - k) / (1 - k);
        return $"cmyk({c:P0}, {m:P0}, {y:P0}, {k:P0})";
    }
}