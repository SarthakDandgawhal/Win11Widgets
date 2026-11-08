using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class CalculatorWidgetViewModel : WidgetViewModelBase
{
    [ObservableProperty]
    private string _display = "0";

    [ObservableProperty]
    private string _expression = "";

    [ObservableProperty]
    private ObservableCollection<string> _history = new();

    private double _currentValue = 0;
    private double _previousValue = 0;
    private string _operation = "";
    private bool _isNewEntry = true;
    private bool _hasDecimal = false;

    public override WidgetType WidgetType => WidgetType.Calculator;

    public CalculatorWidgetViewModel(
        ILogger<CalculatorWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override void LoadState(WidgetState state)
    {
        // Load history if needed
    }

    [RelayCommand]
    public void NumberInput(string number)
    {
        if (_isNewEntry)
        {
            Display = number == "." ? "0." : number;
            _hasDecimal = number == ".";
            _isNewEntry = false;
        }
        else
        {
            if (number == ".")
            {
                if (!_hasDecimal)
                {
                    Display += ".";
                    _hasDecimal = true;
                }
            }
            else
            {
                Display = Display == "0" ? number : Display + number;
            }
        }
    }

    [RelayCommand]
    public void OperationInput(string op)
    {
        if (!_isNewEntry)
        {
            Calculate();
        }
        
        _previousValue = double.Parse(Display);
        _operation = op;
        _isNewEntry = true;
        _hasDecimal = false;
        Expression = $"{_previousValue} {GetOperationSymbol(op)}";
    }

    [RelayCommand]
    public void Equals()
    {
        Calculate();
        _operation = "";
        _isNewEntry = true;
        Expression = "";
    }

    [RelayCommand]
    public void Clear()
    {
        Display = "0";
        Expression = "";
        _currentValue = 0;
        _previousValue = 0;
        _operation = "";
        _isNewEntry = true;
        _hasDecimal = false;
    }

    [RelayCommand]
    public void ClearEntry()
    {
        Display = "0";
        _isNewEntry = true;
        _hasDecimal = false;
    }

    [RelayCommand]
    public void Backspace()
    {
        if (!_isNewEntry && Display.Length > 1)
        {
            Display = Display.Substring(0, Display.Length - 1);
        }
        else
        {
            Display = "0";
            _isNewEntry = true;
        }
    }

    [RelayCommand]
    public void ToggleSign()
    {
        if (!_isNewEntry && Display != "0")
        {
            if (Display.StartsWith("-"))
                Display = Display.Substring(1);
            else
                Display = "-" + Display;
        }
    }

    [RelayCommand]
    public void Percent()
    {
        if (!_isNewEntry)
        {
            var value = double.Parse(Display) / 100;
            Display = value.ToString();
        }
    }

    private void Calculate()
    {
        if (string.IsNullOrEmpty(_operation)) return;

        _currentValue = double.Parse(Display);
        double result = 0;

        try
        {
            result = _operation switch
            {
                "+" => _previousValue + _currentValue,
                "-" => _previousValue - _currentValue,
                "*" => _previousValue * _currentValue,
                "/" => _currentValue != 0 ? _previousValue / _currentValue : double.NaN,
                _ => _currentValue
            };

            var historyEntry = $"{_previousValue} {GetOperationSymbol(_operation)} {_currentValue} = {result}";
            History.Insert(0, historyEntry);
            if (History.Count > 50) History.RemoveAt(History.Count - 1);

            Display = result.ToString();
            _previousValue = result;
        }
        catch
        {
            Display = "Error";
        }
    }

    private string GetOperationSymbol(string op) => op switch
    {
        "+" => "+",
        "-" => "−",
        "*" => "×",
        "/" => "÷",
        _ => op
    };
}