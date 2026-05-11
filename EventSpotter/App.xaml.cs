using EventSpotter.Services;

namespace EventSpotter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        InitializeSupabase();
    }

    private async void InitializeSupabase()
    {
        try
        {
            await SupabaseService.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Supabase init failed: {ex.Message}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}