using EventSpotter.Models;
using EventSpotter.Services;

namespace EventSpotter.Views;

public partial class AdminPage : ContentPage
{
    private List<EventModel> _events = new();
    private EventModel _editingEvent = null;
    private string _activeTab = "events";

    public AdminPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CheckAdminAccess();
    }

    private async Task CheckAdminAccess()
    {
        AdminLoader.IsVisible = true;
        AdminLoader.IsRunning = true;

        // Check if logged in and is admin
        if (!SupabaseService.IsLoggedIn)
        {
            ShowAccessDenied();
            return;
        }

        if (!SupabaseService.IsAdmin)
        {
            ShowAccessDenied();
            return;
        }

        AdminLoader.IsVisible = false;
        AdminLoader.IsRunning = false;

        // Default to events tab
        await LoadEventsTab();
        ShowTab("events");
    }

    private void ShowAccessDenied()
    {
        AdminLoader.IsVisible = false;
        AdminLoader.IsRunning = false;
        AccessDeniedLayout.IsVisible = true;
        EventsTab.IsVisible = false;
        UsersTab.IsVisible = false;
        StatsTab.IsVisible = false;
    }

    // ── TAB SWITCHING ─────────────────────────────────────────
    private void ShowTab(string tab)
    {
        _activeTab = tab;

        EventsTab.IsVisible = tab == "events";
        UsersTab.IsVisible  = tab == "users";
        StatsTab.IsVisible  = tab == "stats";

        TabEventsBtn.TextColor = tab == "events"
            ? Color.FromArgb("#ff6b35") : Color.FromArgb("#3a5068");
        TabUsersBtn.TextColor = tab == "users"
            ? Color.FromArgb("#ff6b35") : Color.FromArgb("#3a5068");
        TabStatsBtn.TextColor = tab == "stats"
            ? Color.FromArgb("#ff6b35") : Color.FromArgb("#3a5068");
    }

    private async void OnTabEvents(object sender, EventArgs e)
    {
        ShowTab("events");
        await LoadEventsTab();
    }

    private async void OnTabUsers(object sender, EventArgs e)
    {
        ShowTab("users");
        await LoadUsersTab();
    }

    private async void OnTabStats(object sender, EventArgs e)
    {
        ShowTab("stats");
        await LoadStatsTab();
    }

    // ── EVENTS TAB ────────────────────────────────────────────
    private async Task LoadEventsTab()
    {
        EventsList.Children.Clear();

        try
        {
            var response = await SupabaseService.Client
                .From<EventModel>()
                .Order("start_datetime", Postgrest.Constants.Ordering.Ascending)
                .Get();

            _events = response.Models;
            EventCountLabel.Text = $"{_events.Count} EVENTS IN DATABASE";

            foreach (var ev in _events)
                EventsList.Children.Add(CreateEventRow(ev));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private View CreateEventRow(EventModel ev)
    {
        var border = new Border
        {
            Stroke = Color.FromArgb("#1AFFFFFF"),
            StrokeThickness = 1,
            BackgroundColor = Color.FromArgb("#0D080D1A"),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 4)
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        {
            CornerRadius = new CornerRadius(3)
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Info
        var info = new VerticalStackLayout { Spacing = 3 };
        info.Children.Add(new Label
        {
            Text = ev.Title ?? "Untitled",
            TextColor = Colors.White,
            FontFamily = "OpenSansRegular",
            FontSize = 13
        });
        info.Children.Add(new Label
        {
            Text = $"{ev.StartDateTime:dd MMM yyyy}  ·  {ev.CategoryName ?? "—"}  ·  {ev.CityName ?? "—"}",
            TextColor = Color.FromArgb("#3a5068"),
            FontFamily = "OpenSansRegular",
            FontSize = 11
        });
        if (ev.IsFree)
        {
            info.Children.Add(new Label
            {
                Text = "FREE",
                TextColor = Color.FromArgb("#06d6a0"),
                FontSize = 9
            });
        }

        grid.Children.Add(info);

        // Action buttons
        var actions = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Center };

        var editBtn = new Button
        {
            Text = "EDIT",
            BackgroundColor = Color.FromArgb("#1500D4FF"),
            TextColor = Color.FromArgb("#00d4ff"),
            FontFamily = "OpenSansRegular",
            FontSize = 10,
            CornerRadius = 2,
            HeightRequest = 32,
            Padding = new Thickness(12, 0)
        };
        editBtn.Clicked += (s, e) => OpenEditForm(ev);

        var deleteBtn = new Button
        {
            Text = "DELETE",
            BackgroundColor = Color.FromArgb("#15FF0000"),
            TextColor = Color.FromArgb("#ff4d4d"),
            FontFamily = "OpenSansRegular",
            FontSize = 10,
            CornerRadius = 2,
            HeightRequest = 32,
            Padding = new Thickness(12, 0)
        };
        deleteBtn.Clicked += async (s, e) => await DeleteEvent(ev);

        actions.Children.Add(editBtn);
        actions.Children.Add(deleteBtn);

        Grid.SetColumn(actions, 1);
        grid.Children.Add(actions);

        border.Content = grid;
        return border;
    }

    private void OnAddEventClicked(object sender, EventArgs e)
    {
        _editingEvent = null;
        FormTitle.Text = "ADD NEW EVENT";
        FormTitle_Input.Text = "";
        FormImage_Input.Text = "";
        FormStart_Input.Text = "";
        FormSource_Input.Text = "";
        FormDesc_Input.Text = "";
        FormFree_Check.IsChecked = false;
        EventFormBorder.IsVisible = true;
    }

    private void OpenEditForm(EventModel ev)
    {
        _editingEvent = ev;
        FormTitle.Text = $"EDITING: {ev.Title?.ToUpper()}";
        FormTitle_Input.Text  = ev.Title ?? "";
        FormImage_Input.Text  = ev.ImageUrl ?? "";
        FormStart_Input.Text  = ev.StartDateTime.ToString("yyyy-MM-dd HH:mm");
        FormSource_Input.Text = ev.SourceUrl ?? "";
        FormDesc_Input.Text   = ev.Description ?? "";
        FormFree_Check.IsChecked = ev.IsFree;
        EventFormBorder.IsVisible = true;
    }

    private void OnCancelForm(object sender, EventArgs e)
    {
        EventFormBorder.IsVisible = false;
        _editingEvent = null;
    }

    private async void OnSaveEvent(object sender, EventArgs e)
    {
        var title = FormTitle_Input.Text?.Trim();
        if (string.IsNullOrEmpty(title))
        {
            await DisplayAlert("Error", "Event title is required", "OK");
            return;
        }

        FormSaveBtn.Text = "SAVING...";
        FormSaveBtn.IsEnabled = false;

        try
        {
            if (!DateTime.TryParse(FormStart_Input.Text, out var startDate))
                startDate = DateTime.UtcNow.AddDays(30);

            if (_editingEvent != null)
            {
                // Update existing
                _editingEvent.Title       = title;
                _editingEvent.ImageUrl    = FormImage_Input.Text?.Trim();
                _editingEvent.StartDateTime = startDate;
                _editingEvent.SourceUrl   = FormSource_Input.Text?.Trim();
                _editingEvent.Description = FormDesc_Input.Text?.Trim();
                _editingEvent.IsFree      = FormFree_Check.IsChecked;

                await SupabaseService.Client
                    .From<EventModel>()
                    .Update(_editingEvent);
            }
            else
            {
                // Insert new
                await SupabaseService.Client
                    .From<EventModel>()
                    .Insert(new EventModel
                    {
                        Title         = title,
                        ImageUrl      = FormImage_Input.Text?.Trim(),
                        StartDateTime = startDate,
                        SourceUrl     = FormSource_Input.Text?.Trim(),
                        Description   = FormDesc_Input.Text?.Trim(),
                        IsFree        = FormFree_Check.IsChecked
                    });
            }

            EventFormBorder.IsVisible = false;
            _editingEvent = null;
            await LoadEventsTab();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            FormSaveBtn.Text = "SAVE EVENT";
            FormSaveBtn.IsEnabled = true;
        }
    }

    private async Task DeleteEvent(EventModel ev)
    {
        bool confirm = await DisplayAlert(
            "CONFIRM DELETE",
            $"Delete '{ev.Title}'? This cannot be undone.",
            "DELETE", "CANCEL");

        if (!confirm) return;

        try
        {
            await SupabaseService.Client
                .From<EventModel>()
                .Where(x => x.Id == ev.Id)
                .Delete();

            await LoadEventsTab();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ── USERS TAB ─────────────────────────────────────────────
    private async Task LoadUsersTab()
    {
        UsersList.Children.Clear();

        try
        {
            var response = await SupabaseService.Client
                .From<UserProfile>()
                .Get();

            var profiles = response.Models;
            UserCountLabel.Text = $"{profiles.Count} REGISTERED USERS";

            foreach (var profile in profiles)
                UsersList.Children.Add(CreateUserRow(profile));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private View CreateUserRow(UserProfile profile)
    {
        var border = new Border
        {
            Stroke = Color.FromArgb("#1AFFFFFF"),
            StrokeThickness = 1,
            BackgroundColor = Color.FromArgb("#0D080D1A"),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 4)
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        {
            CornerRadius = new CornerRadius(3)
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var info = new VerticalStackLayout { Spacing = 3 };
        info.Children.Add(new Label
        {
            Text = profile.FullName ?? "Unknown User",
            TextColor = Colors.White,
            FontFamily = "OpenSansRegular",
            FontSize = 13
        });
        info.Children.Add(new Label
        {
            Text = $"ID: {profile.Id?[..8]}...  ·  City: {profile.HomeCityName ?? "Not set"}",
            TextColor = Color.FromArgb("#3a5068"),
            FontFamily = "OpenSansRegular",
            FontSize = 11
        });
        if (profile.Preferences?.Length > 0)
        {
            info.Children.Add(new Label
            {
                Text = $"Interests: {string.Join(", ", profile.Preferences)}",
                TextColor = Color.FromArgb("#3a5068"),
                FontFamily = "OpenSansRegular",
                FontSize = 10
            });
        }

        grid.Children.Add(info);

        var actions = new HorizontalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center
        };

        var viewBtn = new Button
        {
            Text = "VIEW PREFS",
            BackgroundColor = Color.FromArgb("#1500D4FF"),
            TextColor = Color.FromArgb("#00d4ff"),
            FontFamily = "OpenSansRegular",
            FontSize = 10,
            CornerRadius = 2,
            HeightRequest = 32,
            Padding = new Thickness(12, 0)
        };
        viewBtn.Clicked += async (s, e) =>
            await DisplayAlert(
                profile.FullName ?? "User",
                $"City ID: {profile.HomeCityId}\n" +
                $"Preferences: {string.Join(", ", profile.Preferences ?? Array.Empty<string>())}\n" +
                $"Joined: {profile.CreatedAt:dd MMM yyyy}",
                "CLOSE");

        var deleteBtn = new Button
        {
            Text = "DELETE",
            BackgroundColor = Color.FromArgb("#15FF0000"),
            TextColor = Color.FromArgb("#ff4d4d"),
            FontFamily = "OpenSansRegular",
            FontSize = 10,
            CornerRadius = 2,
            HeightRequest = 32,
            Padding = new Thickness(12, 0)
        };
        deleteBtn.Clicked += async (s, e) => await DeleteUser(profile);

        actions.Children.Add(viewBtn);
        actions.Children.Add(deleteBtn);
        Grid.SetColumn(actions, 1);
        grid.Children.Add(actions);

        border.Content = grid;
        return border;
    }

    private async Task DeleteUser(UserProfile profile)
    {
        bool confirm = await DisplayAlert(
            "CONFIRM DELETE",
            $"Delete user '{profile.FullName}'? This cannot be undone.",
            "DELETE", "CANCEL");

        if (!confirm) return;

        try
        {
            await SupabaseService.Client
                .From<UserProfile>()
                .Where(u => u.Id == profile.Id)
                .Delete();

            await LoadUsersTab();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ── STATS TAB ─────────────────────────────────────────────
    private async Task LoadStatsTab()
    {
        try
        {
            var events = await SupabaseService.Client
                .From<EventModel>().Get();
            var profiles = await SupabaseService.Client
                .From<UserProfile>().Get();
            var bookmarks = await SupabaseService.Client
                .From<BookmarkModel>().Get();

            StatEvents.Text    = events.Models.Count.ToString();
            StatUsers.Text     = profiles.Models.Count.ToString();
            StatBookmarks.Text = bookmarks.Models.Count.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}