using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class CalendarWidgetViewModel : WidgetViewModelBase
{
    [ObservableProperty]
    private DateTime _currentMonth = DateTime.Now;

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _days = new();

    [ObservableProperty]
    private string _monthYear = "";

    public override WidgetType WidgetType => WidgetType.Calendar;

    public CalendarWidgetViewModel(
        ILogger<CalendarWidgetViewModel> logger,
        WidgetManagerService widgetManager,
        WidgetPersistenceService persistence,
        SettingsService settings)
        : base(logger, widgetManager, persistence, settings)
    {
    }

    protected override void LoadState(WidgetState state)
    {
        UpdateCalendar();
    }

    [RelayCommand]
    public void PreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        UpdateCalendar();
    }

    [RelayCommand]
    public void NextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        UpdateCalendar();
    }

    [RelayCommand]
    public void Today()
    {
        CurrentMonth = DateTime.Now;
        UpdateCalendar();
    }

    private void UpdateCalendar()
    {
        MonthYear = CurrentMonth.ToString("MMMM yyyy");
        Days.Clear();

        var firstDay = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
        var startDayOfWeek = (int)firstDay.DayOfWeek;

        // Previous month days
        var prevMonth = CurrentMonth.AddMonths(-1);
        var prevMonthDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        for (int i = startDayOfWeek - 1; i >= 0; i--)
        {
            Days.Add(new CalendarDay
            {
                Day = prevMonthDays - i,
                IsCurrentMonth = false,
                Date = new DateTime(prevMonth.Year, prevMonth.Month, prevMonthDays - i)
            });
        }

        // Current month days
        var today = DateTime.Today;
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
            Days.Add(new CalendarDay
            {
                Day = day,
                IsCurrentMonth = true,
                IsToday = date == today,
                Date = date
            });
        }

        // Next month days to fill grid
        var totalCells = Days.Count;
        var remainingCells = (6 * 7) - totalCells;
        for (int day = 1; day <= remainingCells; day++)
        {
            Days.Add(new CalendarDay
            {
                Day = day,
                IsCurrentMonth = false,
                Date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, daysInMonth).AddDays(day)
            });
        }
    }
}

public partial class CalendarDay : ObservableObject
{
    [ObservableProperty] public int Day;
    [ObservableProperty] public bool IsCurrentMonth = true;
    [ObservableProperty] public bool IsToday = false;
    [ObservableProperty] public DateTime Date;
    [ObservableProperty] public bool HasEvent = false;
}