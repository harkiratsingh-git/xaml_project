using EventSpotter.Views;

namespace EventSpotter;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register all routes for GoToAsync navigation
        Routing.RegisterRoute("LoginPage",       typeof(LoginPage));
        Routing.RegisterRoute("MainSitePage",    typeof(MainSitePage));
        Routing.RegisterRoute("PreferencesPage", typeof(PreferencesPage));
        Routing.RegisterRoute("EventDetailPage", typeof(EventDetailPage));
        Routing.RegisterRoute("CartPage",        typeof(CartPage));
        Routing.RegisterRoute("BookmarksPage",   typeof(BookmarksPage));
        Routing.RegisterRoute("AdminPage",       typeof(AdminPage));
    }
}