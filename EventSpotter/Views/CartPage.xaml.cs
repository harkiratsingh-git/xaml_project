using EventSpotter.Models;
using EventSpotter.Services;

namespace EventSpotter.Views;

public partial class CartPage : ContentPage
{
    private List<CartModel> _cart = new();
    private const double TicketPrice = 24.99;

    public CartPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCartData();
    }

    private async Task LoadCartData()
    {
        _cart.Clear();

        try
        {
            var user = SupabaseService.CurrentUser;
            if (user != null)
            {
                var response = await SupabaseService.Client
                    .From<CartModel>()
                    .Where(c => c.UserId == user.Id)
                    .Get();
                _cart = response.Models;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        EmptyLayout.IsVisible = !_cart.Any() && !SuccessLayout.IsVisible;
        CartContent.IsVisible = _cart.Any() && !SuccessLayout.IsVisible;
        ItemCountLabel.Text = $"{_cart.Count} ITEM{(_cart.Count != 1 ? "S" : "")}";

        RenderItems();
        CalculateTotal();
    }

    private void RenderItems()
    {
        ItemsStack.Children.Clear();
        SummaryList.Children.Clear();

        foreach (var item in _cart)
        {
            ItemsStack.Children.Add(CreateCartItemRow(item));

            // Summary row
            var summaryRow = new Grid { ColumnDefinitions = "*, Auto" };
            summaryRow.Children.Add(new Label
            {
                Text = $"Item #{item.EventId} ×{item.Quantity}",
                FontSize = 11,
                TextColor = Color.FromArgb("#3a5068"),
                FontFamily = "OpenSansRegular"
            });
            var priceLabel = new Label
            {
                Text = $"€ {TicketPrice * item.Quantity:N2}",
                TextColor = Colors.White,
                FontFamily = "OpenSansRegular"
            };
            Grid.SetColumn(priceLabel, 1);
            summaryRow.Children.Add(priceLabel);
            SummaryList.Children.Add(summaryRow);
        }
    }

    private View CreateCartItemRow(CartModel item)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#E6080D1A"),
            BorderColor = Color.FromArgb("#1AFFFFFF"),
            Padding = 0,
            CornerRadius = 3,
            HasShadow = false
        };

        var grid = new Grid { ColumnDefinitions = "3, *, Auto" };

        // Color strip
        grid.Children.Add(new BoxView { Color = Color.FromArgb("#00d4ff") });

        // Details
        var details = new VerticalStackLayout
        {
            Padding = 14,
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center
        };
        details.Children.Add(new Label
        {
            Text = $"Event #{item.EventId}",
            TextColor = Colors.White,
            FontFamily = "OpenSansRegular",
            FontSize = 13
        });
        details.Children.Add(new Label
        {
            Text = $"Qty: {item.Quantity}",
            TextColor = Color.FromArgb("#3a5068"),
            FontSize = 10,
            FontFamily = "OpenSansRegular"
        });
        Grid.SetColumn(details, 1);
        grid.Children.Add(details);

        // Actions
        var actions = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Padding = 10,
            Spacing = 8
        };
        actions.Children.Add(new Label
        {
            Text = $"€ {TicketPrice * item.Quantity:N2}",
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.End,
            FontFamily = "OpenSansRegular"
        });

        var removeBtn = new Button
        {
            Text = "REMOVE",
            TextColor = Color.FromArgb("#3a5068"),
            BackgroundColor = Colors.Transparent,
            FontSize = 9,
            FontFamily = "OpenSansRegular"
        };
        removeBtn.Clicked += async (s, e) => await RemoveItem(item);
        actions.Children.Add(removeBtn);

        Grid.SetColumn(actions, 2);
        grid.Children.Add(actions);

        frame.Content = grid;
        return frame;
    }

    private async Task RemoveItem(CartModel item)
    {
        try
        {
            var user = SupabaseService.CurrentUser;
            if (user != null)
            {
                await SupabaseService.Client
                    .From<CartModel>()
                    .Where(c => c.CartId == item.CartId)
                    .Delete();
            }

            _cart.Remove(item);
            RefreshUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void CalculateTotal()
    {
        double total = _cart.Sum(x => TicketPrice * x.Quantity);
        TotalPriceLabel.Text = $"€ {total:N2}";
    }

    private async void OnCheckoutClicked(object sender, EventArgs e)
    {
        try
        {
            var user = SupabaseService.CurrentUser;
            if (user != null)
            {
                await SupabaseService.Client
                    .From<CartModel>()
                    .Where(c => c.UserId == user.Id)
                    .Delete();
            }

            CartContent.IsVisible = false;
            SuccessLayout.IsVisible = true;
            await SuccessProgress.ProgressTo(0, 3000, Easing.Linear);
            await Shell.Current.GoToAsync("//MainSitePage");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}