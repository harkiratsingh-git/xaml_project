using EventSpotter.Models;
using EventSpotter.Services;

namespace EventSpotter.Views;

public partial class MainSitePage : ContentPage
{
    private bool _isFreeOnly = false;
    private List<EventModel> _allEvents = new();
    private List<EventModel> _filtered = new();

    public MainSitePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadData();
    }

    private async Task LoadData()
    {
        LoadingSpinner.IsVisible = true;
        LoadingSpinner.IsRunning = true;
        EventGrid.Children.Clear();
        EmptyState.IsVisible = false;

        try
        {
            var response = await SupabaseService.Client
                .From<EventModel>()
                .Order("start_datetime", Postgrest.Constants.Ordering.Ascending)
                .Get();

            _allEvents = response.Models;

            // Populate country picker
            var countries = _allEvents
                .Select(e => e.CountryName)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CountryPicker.Items.Clear();
            CountryPicker.Items.Add("ALL COUNTRIES");
            foreach (var c in countries)
                CountryPicker.Items.Add(c);
            CountryPicker.SelectedIndex = 0;

            // Populate category picker
            var cats = _allEvents
                .Select(e => e.CategoryName)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            CategoryPicker.Items.Clear();
            CategoryPicker.Items.Add("ALL TYPES");
            foreach (var c in cats)
                CategoryPicker.Items.Add(c);
            CategoryPicker.SelectedIndex = 0;

            // Update stats label
            StatsLabel.Text = $"{_allEvents.Count} EVENTS · {countries.Count} COUNTRIES";
            EventCountLabel.Text = $"{_allEvents.Count} EVENTS";

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }

    private void ApplyFilters()
    {
        var search = SearchEntry.Text?.ToLower() ?? "";
        var country = CountryPicker.SelectedIndex > 0
            ? CountryPicker.Items[CountryPicker.SelectedIndex] : null;
        var category = CategoryPicker.SelectedIndex > 0
            ? CategoryPicker.Items[CategoryPicker.SelectedIndex] : null;

        _filtered = _allEvents.Where(e =>
        {
            if (_isFreeOnly && !e.IsFree) return false;
            if (!string.IsNullOrEmpty(country) && e.CountryName != country) return false;
            if (!string.IsNullOrEmpty(category) && e.CategoryName != category) return false;
            if (!string.IsNullOrEmpty(search) &&
                !e.Title?.ToLower().Contains(search) == true) return false;
            return true;
        }).ToList();

        RenderCards();
    }

    private void RenderCards()
    {
        EventGrid.Children.Clear();
        EmptyState.IsVisible = !_filtered.Any();
        EventCountLabel.Text = $"{_filtered.Count} EVENTS";

        foreach (var ev in _filtered)
            EventGrid.Children.Add(CreateEventCard(ev));
    }

    private View CreateEventCard(EventModel ev)
    {
        var card = new Frame
        {
            BackgroundColor = Color.FromArgb("#0D0A1228"),
            BorderColor = Color.FromArgb("#1AFFFFFF"),
            CornerRadius = 4,
            Padding = 0,
            Margin = 8,
            WidthRequest = 280,
            HasShadow = false
        };

        var content = new VerticalStackLayout();

        // Image
        var imgGrid = new Grid { HeightRequest = 160 };
        if (!string.IsNullOrEmpty(ev.ImageUrl))
        {
            imgGrid.Children.Add(new Image
            {
                Source = ev.ImageUrl,
                Aspect = Aspect.AspectFill,
                Opacity = 0.6
            });
        }

        // Category badge
        var badge = new Frame
        {
            BackgroundColor = Color.FromArgb("#1A00D4FF"),
            BorderColor = Color.FromArgb("#3300D4FF"),
            CornerRadius = 10,
            Padding = new Thickness(8, 3),
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(10, 10, 0, 0),
            HasShadow = false
        };
        badge.Content = new Label
        {
            Text = ev.CategoryName?.ToUpper() ?? "EVENT",
            TextColor = Color.FromArgb("#00d4ff"),
            FontSize = 9
        };
        imgGrid.Children.Add(badge);

        content.Children.Add(imgGrid);

        // Body
        var body = new VerticalStackLayout { Padding = 14, Spacing = 6 };

        body.Children.Add(new Label
        {
            Text = ev.Title,
            TextColor = Colors.White,
            FontFamily = "OpenSansRegular",
            FontSize = 13,
            LineBreakMode = LineBreakMode.WordWrap
        });

        body.Children.Add(new Label
        {
            Text = $"{ev.VenueName} · {ev.CityName}",
            TextColor = Color.FromArgb("#3a5068"),
            FontSize = 11
        });

        body.Children.Add(new Label
        {
            Text = ev.FormattedDate,
            TextColor = Color.FromArgb("#3a5068"),
            FontSize = 10
        });

        if (ev.IsFree)
        {
            body.Children.Add(new Label
            {
                Text = "FREE ENTRY",
                TextColor = Color.FromArgb("#06d6a0"),
                FontSize = 9
            });
        }

        content.Children.Add(body);
        card.Content = content;

        // Tap to navigate to detail
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, e) =>
        {
            await Shell.Current.GoToAsync($"EventDetailPage?id={ev.Id}");
        };
        card.GestureRecognizers.Add(tap);

        return card;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
    private void OnFilterChanged(object sender, EventArgs e) => ApplyFilters();

    private void OnFreeToggled(object sender, EventArgs e)
    {
        _isFreeOnly = !_isFreeOnly;
        FreeBtn.Text = _isFreeOnly ? "FREE: ON" : "FREE: OFF";
        FreeBtn.TextColor = _isFreeOnly
            ? Color.FromArgb("#06d6a0")
            : Color.FromArgb("#3a5068");
        ApplyFilters();
    }

    private void OnHideHero(object sender, EventArgs e)
    {
        HeroSection.IsVisible = false;
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//LandingPage");

    private async void OnLoginClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//LoginPage");

    private async void OnPrefsClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("PreferencesPage");
}