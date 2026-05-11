using EventSpotter.Models;
using EventSpotter.Services;

namespace EventSpotter.Views;

public partial class BookmarksPage : ContentPage
{
    private List<EventModel> _bookmarkedEvents = new();

    public BookmarksPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBookmarks();
    }

    private async Task LoadBookmarks()
    {
        LoadingLayout.IsVisible = true;
        BookmarksGrid.Children.Clear();
        _bookmarkedEvents.Clear();

        try
        {
            var user = SupabaseService.CurrentUser;

            if (user != null)
            {
                // Get bookmark records for this user
                var bookmarkResult = await SupabaseService.Client
                    .From<BookmarkModel>()
                    .Where(b => b.UserId == user.Id)
                    .Get();

                var eventIds = bookmarkResult.Models
                    .Select(b => b.EventId)
                    .ToList();

                // Fetch each event
                foreach (var id in eventIds)
                {
                    var ev = await SupabaseService.Client
                        .From<EventModel>()
                        .Where(e => e.Id == id)
                        .Single();
                    if (ev != null)
                        _bookmarkedEvents.Add(ev);
                }
            }
            else
            {
                // Guest fallback — stored as comma-separated ids
                var saved = Preferences.Default.Get("bookmarks", "");
                if (!string.IsNullOrEmpty(saved))
                {
                    var ids = saved.Split(',')
                        .Select(s => int.TryParse(s, out var n) ? n : -1)
                        .Where(n => n > 0)
                        .ToList();

                    foreach (var id in ids)
                    {
                        var ev = await SupabaseService.Client
                            .From<EventModel>()
                            .Where(e => e.Id == id)
                            .Single();
                        if (ev != null)
                            _bookmarkedEvents.Add(ev);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

        RenderGrid();
    }

    private void RenderGrid()
    {
        LoadingLayout.IsVisible = false;
        EmptyLayout.IsVisible = !_bookmarkedEvents.Any();
        CountLabel.Text = $"{_bookmarkedEvents.Count} BOOKMARK{(_bookmarkedEvents.Count != 1 ? "S" : "")}";

        BookmarksGrid.Children.Clear();
        foreach (var ev in _bookmarkedEvents)
            BookmarksGrid.Children.Add(CreateBookmarkCard(ev));
    }

    private View CreateBookmarkCard(EventModel ev)
    {
        var card = new Frame
        {
            BackgroundColor = Color.FromArgb("#F2080D1A"),
            BorderColor = Color.FromArgb("#1AFFFFFF"),
            CornerRadius = 4,
            Padding = 0,
            Margin = 8,
            WidthRequest = 300,
            HasShadow = false
        };

        var layout = new Grid { RowDefinitions = "160, Auto" };

        // Image
        var imgGrid = new Grid();
        if (!string.IsNullOrEmpty(ev.ImageUrl))
            imgGrid.Children.Add(new Image
            {
                Source = ev.ImageUrl,
                Aspect = Aspect.AspectFill,
                Opacity = 0.6
            });

        // Gradient overlay
        var overlay = new BoxView();
        overlay.Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(Colors.Transparent, 0.4f),
                new GradientStop(Color.FromArgb("#080D1A"), 1.0f)
            },
            new Point(0, 0),
            new Point(0, 1));
        imgGrid.Children.Add(overlay);

        layout.Children.Add(imgGrid);
        Grid.SetRow(imgGrid, 0);

        // Body
        var body = new VerticalStackLayout { Padding = 14, Spacing = 8 };

        body.Children.Add(new Label
        {
            Text = ev.CategoryName?.ToUpper() ?? "EVENT",
            TextColor = Color.FromArgb("#00d4ff"),
            FontSize = 9
        });

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

        // Footer with view + remove buttons
        var footer = new Grid { ColumnDefinitions = "*, Auto, Auto", Margin = new Thickness(0, 4, 0, 0) };

        footer.Children.Add(new Label
        {
            Text = ev.FormattedDate,
            TextColor = Color.FromArgb("#3a5068"),
            FontSize = 10,
            VerticalOptions = LayoutOptions.Center
        });

        var viewBtn = new Button
        {
            Text = "VIEW →",
            TextColor = Color.FromArgb("#00d4ff"),
            BackgroundColor = Color.FromArgb("#1500D4FF"),
            FontSize = 10,
            HeightRequest = 32,
            CornerRadius = 2,
            FontFamily = "OpenSansRegular",
            Margin = new Thickness(4, 0)
        };
        viewBtn.Clicked += async (s, e) =>
            await Shell.Current.GoToAsync($"EventDetailPage?id={ev.Id}");

        var removeBtn = new Button
        {
            Text = "REMOVE",
            TextColor = Color.FromArgb("#ff4d4d"),
            BackgroundColor = Color.FromArgb("#15FF0000"),
            FontSize = 10,
            HeightRequest = 32,
            CornerRadius = 2,
            FontFamily = "OpenSansRegular"
        };
        removeBtn.Clicked += async (s, e) => await RemoveBookmark(ev.Id);

        Grid.SetColumn(viewBtn, 1);
        Grid.SetColumn(removeBtn, 2);
        footer.Children.Add(viewBtn);
        footer.Children.Add(removeBtn);

        body.Children.Add(footer);
        layout.Children.Add(body);
        Grid.SetRow(body, 1);

        card.Content = layout;
        return card;
    }

    private async Task RemoveBookmark(int eventId)
    {
        try
        {
            var user = SupabaseService.CurrentUser;
            if (user != null)
            {
                await SupabaseService.Client
                    .From<BookmarkModel>()
                    .Where(b => b.UserId == user.Id && b.EventId == eventId)
                    .Delete();
            }
            else
            {
                var saved = Preferences.Default.Get("bookmarks", "");
                var ids = saved.Split(',')
                    .Where(s => s != eventId.ToString())
                    .ToList();
                Preferences.Default.Set("bookmarks", string.Join(",", ids));
            }

            _bookmarkedEvents.RemoveAll(x => x.Id == eventId);
            RenderGrid();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MainSitePage");
}