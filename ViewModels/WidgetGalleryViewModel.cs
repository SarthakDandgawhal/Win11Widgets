using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Win11Widgets.Models;
using Win11Widgets.Services;

namespace Win11Widgets.ViewModels;

public partial class WidgetGalleryViewModel : ObservableObject
{
    private readonly WidgetManagerService _widgetManager;
    private readonly ILogger<WidgetGalleryViewModel> _logger;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string _selectedCategory = "All";

    public WidgetGalleryViewModel(WidgetManagerService widgetManager, ILogger<WidgetGalleryViewModel> logger)
    {
        _widgetManager = widgetManager;
        _logger = logger;
    }

    public IEnumerable<WidgetDefinition> AllWidgets => _widgetManager.WidgetDefinitions;

    public IEnumerable<string> Categories => new[] { "All" }
        .Concat(_widgetManager.WidgetDefinitions.Select(d => d.Category).Distinct().OrderBy(c => c));

    public IEnumerable<WidgetDefinition> FilteredWidgets
    {
        get
        {
            var query = _widgetManager.WidgetDefinitions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                query = query.Where(w => 
                    w.Name.ToLower().Contains(search) || 
                    w.Description.ToLower().Contains(search));
            }

            if (SelectedCategory != "All")
            {
                query = query.Where(w => w.Category == SelectedCategory);
            }

            return query.OrderBy(w => w.Category).ThenBy(w => w.Name);
        }
    }

    partial void OnSearchTextChanged(string value) => OnPropertyChanged(nameof(FilteredWidgets));
    partial void OnSelectedCategoryChanged(string value) => OnPropertyChanged(nameof(FilteredWidgets));

    [RelayCommand]
    public void AddWidget(WidgetDefinition definition)
    {
        _widgetManager.CreateWidget(definition.Type);
    }
}