namespace EventSpotter.Views;

public partial class LandingPage : ContentPage
{
    private CancellationTokenSource _blinkCts;

    public LandingPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _blinkCts = new CancellationTokenSource();
        StartBlinkingAnimation(_blinkCts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
        _blinkCts = null;
    }

    private async void StartBlinkingAnimation(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await StatusDot.FadeTo(0.2, 600);
            if (token.IsCancellationRequested) break;
            await StatusDot.FadeTo(1.0, 600);
        }
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        PortalCard.TranslateTo(0, -6, 250, Easing.CubicOut);
        PortalArrow.FadeTo(1, 200);
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        PortalCard.TranslateTo(0, 0, 250, Easing.CubicIn);
        PortalArrow.FadeTo(0, 200);
    }

    private async void OnExploreTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}