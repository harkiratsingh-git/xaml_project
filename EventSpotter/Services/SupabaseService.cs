using Supabase;

namespace EventSpotter.Services
{
    public static class SupabaseService
    {
        private const string SupabaseUrl = "https://cgiostxmsiczlpzwuhiw.supabase.co";
        private const string SupabaseKey = "sb_publishable_zlD_MLdb6PoQSGUJgbK-Ig_8hwsvQtt";

        private static Supabase.Client _client;

        public static Supabase.Client Client
        {
            get
            {
                if (_client == null)
                {
                    var options = new SupabaseOptions
                    {
                        AutoConnectRealtime = true,
                        AutoRefreshToken = true
                    };
                    _client = new Supabase.Client(SupabaseUrl, SupabaseKey, options);
                }
                return _client;
            }
        }

        public static async Task InitializeAsync()
        {
            await Client.InitializeAsync();
        }

        // Helper to get current logged in user
        public static Supabase.Gotrue.User CurrentUser =>
            Client.Auth.CurrentUser;

        // Helper to check if logged in
        public static bool IsLoggedIn =>
            Client.Auth.CurrentUser != null;

        // Helper to check if user is admin
        public static bool IsAdmin =>
            Client.Auth.CurrentUser?.UserMetadata
                .ContainsKey("is_admin") == true &&
            Client.Auth.CurrentUser.UserMetadata["is_admin"]
                .ToString() == "True";
    }
}