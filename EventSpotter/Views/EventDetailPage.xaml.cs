using EventSpotter.Models;
using EventSpotter.Services;

namespace EventSpotter.Views;

[QueryProperty(nameof(EventId), "id")]
public partial class EventDetailPage : ContentPage
{
    public string EventId { get; set; }
    private EventModel _event;
    private int _tickets = 1;
    private const double TicketPrice = 24.99;
    private int _activeNotifIndex = 0;
    private List<(string Icon, string Text)> _currentNotifs;
    private IDispatcherTimer _notifTimer;

    public EventDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEventData();
        UpdateCartBadge();
        StartNotificationTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _notifTimer?.Stop();
    }

    private async Task LoadEventData()
    {
        try
        {
            var response = await SupabaseService.Client
                .From<EventModel>()
                .Where(x => x.Id == int.Parse(EventId))
                .Single();

            _event = response;

            // Bind to UI
            TitleLabel.Text = _event.Title;
            BadgeLabel.Text = $"✦ {_event.CategoryName?.ToUpper() ?? "EVENT"}";
            CategoryLabel.Text = _event.CategoryName?.ToUpper() ?? "";
            CategoryLabel.TextColor = GetCategoryColor(_event.CategoryName);
            DateLabel.Text = _event.StartDateTime.ToString("dddd, dd MMMM yyyy");
            TimeLabel.Text = _event.StartDateTime.ToString("HH:mm");
            VenueLabel.Text = _event.VenueName ?? "—";
            CityLabel.Text = _event.CityName ?? "—";
            CountryLabel.Text = _event.CountryName ?? "—";
            EntryLabel.Text = _event.IsFree ? "FREE" : "TICKETED";
            EntryLabel.TextColor = _event.IsFree
                ? Color.FromArgb("#06d6a0")
                : Color.FromArgb("#e8f4ff");

            if (_event.IsFree)
            {
                PriceLabel.Text = "FREE";
                PriceLabel.TextColor = Color.FromArgb("#06d6a0");
            }
            else
            {
                PriceLabel.Text = $"€ {TicketPrice:F2}";
                UpdateTotal();
            }

            if (!string.IsNullOrEmpty(_event.ImageUrl))
                EventImage.Source = _event.ImageUrl;

            if (!string.IsNullOrEmpty(_event.Description))
            {
                DescriptionLabel.Text = _event.Description;
                DescriptionLayout.IsVisible = true;
            }

            if (!string.IsNullOrEmpty(_event.SourceUrl))
                OfficialLinkBtn.IsVisible = true;

            SetupNotifications(_event.CategoryName);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load event: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private Color GetCategoryColor(string category) => category switch
    {
        "Music"    => Color.FromArgb("#00d4ff"),
        "Art"      => Color.FromArgb("#b06cff"),
        "History"  => Color.FromArgb("#ffd166"),
        "Science"  => Color.FromArgb("#06d6a0"),
        "Festival" => Color.FromArgb("#ff6b35"),
        "Theatre"  => Color.FromArgb("#ff4d8d"),
        _          => Color.FromArgb("#00d4ff")
    };

    private void SetupNotifications(string category)
    {
        _currentNotifs = category switch
        {
            "Music" => new List<(string, string)>
            {
                ("🎤", "Special guest artist just announced"),
                ("🔥", "Only 12% of tickets remaining"),
                ("⭐", "Sold out 3 venues this month")
            },
            "Festival" => new List<(string, string)>
            {
                ("🚗", "Rare exotic vehicle confirmed for display"),
                ("🔥", "Last 50 tickets at this price"),
                ("⚡", "New exhibitor added to the lineup")
            },
            "Art" => new List<(string, string)>
            {
                ("🎨", "Rare original piece added today"),
                ("👁", "Featured in Top 10 exhibitions in Europe"),
            },
            "History" => new List<(string, string)>
            {
                ("🏺", "New artefact on display for first time"),
                ("📜", "Guided expert tour added this weekend"),
            },
            _ => new List<(string, string)>
            {
                ("✦", "Limited spots available"),
                ("🔥", "High demand this week"),
            }
        };

        UpdateNotifUI();
    }

    private void StartNotificationTimer()
    {
        _notifTimer = Dispatcher.CreateTimer();
        _notifTimer.Interval = TimeSpan.FromSeconds(3);
        _notifTimer.Tick += (s, e) =>
        {
            _activeNotifIndex = (_activeNotifIndex + 1) % _currentNotifs.Count;
            UpdateNotifUI();
        };
        _notifTimer.Start();
    }

    private void UpdateNotifUI()
    {
        var notif = _currentNotifs[_activeNotifIndex];
        NotifIcon.Text = notif.Icon;
        NotifText.Text = notif.Text;
    }

    private void UpdateTotal()
    {
        var total = _event?.IsFree == true ? 0 : TicketPrice * _tickets;
        TotalLabel.Text = _event?.IsFree == true ? "FREE" : $"€ {total:F2}";
    }

    private void OnIncreaseTickets(object sender, EventArgs e)
    {
        if (_tickets < 10) _tickets++;
        TicketCountLabel.Text = _tickets.ToString();
        UpdateTotal();
    }

    private void OnDecreaseTickets(object sender, EventArgs e)
    {
        if (_tickets > 1) _tickets--;
        TicketCountLabel.Text = _tickets.ToString();
        UpdateTotal();
    }

    private async void OnAddToCart(object sender, EventArgs e)
    {
        AddBtn.IsEnabled = false;
        AddBtn.Text = "ADDING...";

        try
        {
            var user = SupabaseService.CurrentUser;
            if (user != null)
            {
                // Check if already in cart
                var existing = await SupabaseService.Client
                    .From<CartModel>()
                    .Where(c => c.UserId == user.Id &&
                                c.EventId == _event.Id)
                    .Single();

                if (existing != null)
                {
                    await SupabaseService.Client
                        .From<CartModel>()
                        .Where(c => c.CartId == existing.CartId)
                        .Set(c => c.Quantity, existing.Quantity + _tickets)
                        .Update();
                }
                else
                {
                    await SupabaseService.Client
                        .From<CartModel>()
                        .Insert(new CartModel
                        {
                            UserId = user.Id,
                            EventId = _event.Id,
                            Quantity = _tickets,
                            AddedAt = DateTime.UtcNow
                        });
                }
            }
            else
            {
                // Guest — save to local preferences
                Preferences.Default.Set(
                    $"cart_{_event.Id}",
                    _tickets.ToString());
            }

            AddBtn.Text = "✓ ADDED";
            AddBtn.BackgroundColor = Color.FromArgb("#06d6a0");
            UpdateCartBadge();
            await Task.Delay(2000);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            AddBtn.Text = "ADD TO CART";
            AddBtn.BackgroundColor = Color.FromArgb("#00d4ff");
            AddBtn.IsEnabled = true;
        }
    }

    private async void UpdateCartBadge()
    {
        try
        {
            var user = SupabaseService.CurrentUser;
            int count = 0;
            if (user != null)
            {
                var cartItems = await SupabaseService.Client
                    .From<CartModel>()
                    .Where(c => c.UserId == user.Id)
                    .Get();
                count = cartItems.Models.Count;
            }
            CartBtn.Text = $"CART [{count}]";
        }
        catch { }
    }

    private async void OnCartClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("CartPage");

    private async void OnOfficialLinkClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_event?.SourceUrl))
            await Launcher.OpenAsync(_event.SourceUrl);
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}