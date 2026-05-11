using EventSpotter.Models;
using EventSpotter.Services;

namespace EventSpotter.Views;

public partial class PreferencesPage : ContentPage
{
    private List<CityData> _allCities = new()
    {
        new CityData { City = "Brussels",   Country = "Belgium" },
        new CityData { City = "Antwerp",    Country = "Belgium" },
        new CityData { City = "Ghent",      Country = "Belgium" },
        new CityData { City = "Paris",      Country = "France" },
        new CityData { City = "Lyon",       Country = "France" },
        new CityData { City = "Berlin",     Country = "Germany" },
        new CityData { City = "Munich",     Country = "Germany" },
        new CityData { City = "Amsterdam",  Country = "Netherlands" },
        new CityData { City = "London",     Country = "United Kingdom" },
        new CityData { City = "Manchester", Country = "United Kingdom" },
        new CityData { City = "Madrid",     Country = "Spain" },
        new CityData { City = "Barcelona",  Country = "Spain" },
        new CityData { City = "Rome",       Country = "Italy" },
        new CityData { City = "Milan",      Country = "Italy" },
        new CityData { City = "Vienna",     Country = "Austria" },
        new CityData { City = "Prague",     Country = "Czech Republic" },
    };

    private List<CategoryData> _categories = new()
    {
        new CategoryData { Id = "Music",    Icon = "🎵", Label = "CONCERTS",    Desc = "Live music & gigs" },
        new CategoryData { Id = "Art",      Icon = "🎨", Label = "EXHIBITIONS", Desc = "Galleries & art shows" },
        new CategoryData { Id = "History",  Icon = "🏛", Label = "HISTORICAL",  Desc = "Museums & heritage" },
        new CategoryData { Id = "Science",  Icon = "🔬", Label = "SCIENCE",     Desc = "Tech & science fairs" },
        new CategoryData { Id = "Festival", Icon = "🚗", Label = "EXPOS",       Desc = "Car, food & trade expos" },
        new CategoryData { Id = "Theatre",  Icon = "🎭", Label = "THEATRE",     Desc = "Plays, opera & dance" },
    };

    private List<string> _selectedCategories = new();
    private CityData _selectedCity;

    public PreferencesPage()
    {
        InitializeComponent();
        SetupCategoryGrid();
    }

    private void OnCityInputChanged(object sender, TextChangedEventArgs e)
    {
        var val = e.NewTextValue;
        if (string.IsNullOrWhiteSpace(val) || val.Length < 2)
        {
            SuggestionsFrame.IsVisible = false;
            return;
        }

        var matches = _allCities
            .Where(c => c.City.ToLower().StartsWith(val.ToLower()) ||
                        c.Country.ToLower().StartsWith(val.ToLower()))
            .Take(6)
            .ToList();

        SuggestionsList.Children.Clear();
        foreach (var match in matches)
        {
            var btn = new Button
            {
                Text = $"{match.City}  ({match.Country.ToUpper()})",
                TextColor = Color.FromArgb("#e8f4ff"),
                BackgroundColor = Colors.Transparent,
                FontFamily = "OpenSansRegular",
                HorizontalOptions = LayoutOptions.Fill,
                FontSize = 12
            };
            btn.Clicked += (s, ev) => SelectCity(match);
            SuggestionsList.Children.Add(btn);
        }

        SuggestionsFrame.IsVisible = matches.Any();
    }

    private void SelectCity(CityData city)
    {
        _selectedCity = city;
        SuggestionsFrame.IsVisible = false;
        CityEntry.Text = city.City;
        ConfirmCity.Text = city.City;
        ConfirmCountry.Text = city.Country.ToUpper();
        ConfirmBox.IsVisible = true;
        NextBtn.IsEnabled = true;
        NextBtn.BackgroundColor = Color.FromArgb("#00d4ff");
        NextBtn.TextColor = Color.FromArgb("#03060f");
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        Step1Content.IsVisible = false;
        Step2Content.IsVisible = true;
        StepLabel.Text = "2/2";
        MainTitle.Text = "WHAT ARE YOUR\nINTERESTS?";
        SubTitle.Text = "Pick your event types — we'll send you relevant alerts";
    }

    private void SetupCategoryGrid()
    {
        foreach (var cat in _categories)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#0DFFFFFF"),
                BorderColor = Color.FromArgb("#1AFFFFFF"),
                WidthRequest = 160,
                HeightRequest = 120,
                Margin = 6,
                Padding = 14,
                CornerRadius = 4,
                HasShadow = false
            };

            var stack = new VerticalStackLayout { Spacing = 6 };
            stack.Children.Add(new Label { Text = cat.Icon, FontSize = 24 });
            stack.Children.Add(new Label
            {
                Text = cat.Label,
                TextColor = Colors.White,
                FontFamily = "OpenSansRegular",
                FontSize = 12
            });
            stack.Children.Add(new Label
            {
                Text = cat.Desc,
                TextColor = Color.FromArgb("#3a5068"),
                FontFamily = "OpenSansRegular",
                FontSize = 10
            });

            frame.Content = stack;

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                // Fix: use Contains() not contains()
                if (_selectedCategories.Contains(cat.Id))
                    _selectedCategories.Remove(cat.Id);
                else
                    _selectedCategories.Add(cat.Id);

                frame.BorderColor = _selectedCategories.Contains(cat.Id)
                    ? Color.FromArgb("#00d4ff")
                    : Color.FromArgb("#1AFFFFFF");
                frame.BackgroundColor = _selectedCategories.Contains(cat.Id)
                    ? Color.FromArgb("#1500D4FF")
                    : Color.FromArgb("#0DFFFFFF");
            };

            frame.GestureRecognizers.Add(tap);
            CategoryGrid.Children.Add(frame);
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        SaveBtn.Text = "SAVING...";
        SaveBtn.IsEnabled = false;

        try
        {
            var user = SupabaseService.CurrentUser;
            if (user != null)
            {
                // Get city_id from Supabase
                var cityResult = await SupabaseService.Client
                    .From<Models.UserProfile>()
                    .Get();

                // Upsert profile with preferences
                await SupabaseService.Client
                    .From<Models.UserProfile>()
                    .Upsert(new Models.UserProfile
                    {
                        Id = user.Id,
                        FullName = user.UserMetadata
                            .GetValueOrDefault("full_name")?.ToString() ?? "",
                        Preferences = _selectedCategories.ToArray()
                    });
            }

            // Save to local preferences too
            Preferences.Default.Set("selected_city",
                _selectedCity?.City ?? "");
            Preferences.Default.Set("selected_categories",
                string.Join(",", _selectedCategories));

            await Shell.Current.GoToAsync("//MainSitePage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SaveBtn.Text = "SAVE & LAUNCH →";
            SaveBtn.IsEnabled = true;
        }
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
        Step2Content.IsVisible = false;
        Step1Content.IsVisible = true;
        StepLabel.Text = "1/2";
        MainTitle.Text = "WHERE ARE\nYOU BASED?";
        SubTitle.Text = "Type your city — we'll suggest matches";
    }

    private async void OnSkipClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MainSitePage");
}