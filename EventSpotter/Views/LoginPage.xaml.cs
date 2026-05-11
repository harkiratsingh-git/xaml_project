using EventSpotter.Drawables;
using EventSpotter.Services;

namespace EventSpotter.Views;

public partial class LoginPage : ContentPage
{
    private string _mode = "login";

    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnInputChanged(object sender, TextChangedEventArgs e)
    {
        MainDrawable.EmailLength = EmailEntry.Text?.Length ?? 0;
        MainDrawable.PasswordLength = PasswordEntry.Text?.Length ?? 0;

        // Update password strength in register mode
        if (_mode == "register" && PasswordEntry.Text?.Length > 0)
        {
            var strength = GetPasswordStrength(PasswordEntry.Text);
            StrengthBar.Progress = strength / 100.0;
            StrengthBar.ProgressColor = strength < 40
                ? Color.FromArgb("#ff4d4d")
                : strength < 70
                    ? Color.FromArgb("#ffd166")
                    : Color.FromArgb("#06d6a0");
            StrengthLabel.Text = strength < 40 ? "WEAK"
                : strength < 70 ? "MODERATE" : "STRONG";
            StrengthLabel.TextColor = StrengthBar.ProgressColor;
        }

        SceneCanvas.Invalidate();
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        SubmitBtn.IsEnabled = false;

        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("EMAIL AND PASSWORD REQUIRED");
            SubmitBtn.IsEnabled = true;
            return;
        }

        try
        {
            if (_mode == "login")
            {
                SubmitBtn.Text = "STARTING ENGINE...";
                var session = await SupabaseService.Client.Auth
                    .SignIn(email, password);

                if (session?.User != null)
                {
                    MainDrawable.LoginSuccess = true;
                    SceneCanvas.Invalidate();
                    await Task.Delay(1000);
                    await Shell.Current.GoToAsync("//MainSitePage");
                }
                else
                {
                    MainDrawable.IsBroken = true;
                    SceneCanvas.Invalidate();
                    ShowError("ENGINE FAILURE — CHECK CREDENTIALS");
                    await Task.Delay(2000);
                    MainDrawable.IsBroken = false;
                    SceneCanvas.Invalidate();
                }
            }
            else
            {
                // Register mode
                if (GetPasswordStrength(password) < 60)
                {
                    ShowError("PASSWORD TOO WEAK — FUEL INSUFFICIENT");
                    SubmitBtn.IsEnabled = true;
                    return;
                }

                SubmitBtn.Text = "INITIATING LAUNCH...";
                var session = await SupabaseService.Client.Auth
                    .SignUp(email, password);

                if (session?.User != null)
                {
                    await DisplayAlert(
                        "LAUNCH SEQUENCE INITIATED",
                        "Check your email to confirm your account.",
                        "OK");
                    OnSwitchModeClicked(this, EventArgs.Empty);
                }
                else
                {
                    ShowError("REGISTRATION FAILED — TRY AGAIN");
                }
            }
        }
        catch (Exception ex)
        {
            MainDrawable.IsBroken = true;
            SceneCanvas.Invalidate();
            ShowError(ex.Message.ToUpper());
            await Task.Delay(2000);
            MainDrawable.IsBroken = false;
            SceneCanvas.Invalidate();
        }
        finally
        {
            SubmitBtn.Text = _mode == "login"
                ? "⚑ ENTER THE GRID →"
                : "🚀 LAUNCH →";
            SubmitBtn.IsEnabled = true;
        }
    }

    private void OnSwitchModeClicked(object sender, EventArgs e)
    {
        _mode = _mode == "login" ? "register" : "login";
        TitleLabel.Text = _mode == "login" ? "SIGN IN" : "CREATE ACCOUNT";
        SubmitBtn.Text = _mode == "login"
            ? "⚑ ENTER THE GRID →"
            : "🚀 LAUNCH →";
        SwitchModeBtn.Text = _mode == "login"
            ? "NO ACCOUNT? JOIN THE GRID →"
            : "← BACK TO LOGIN";
        StrengthLayout.IsVisible = _mode == "register";
        MainDrawable.Mode = _mode;
        ErrorLabel.IsVisible = false;
        SceneCanvas.Invalidate();
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = $"⚠ {message}";
        ErrorLabel.IsVisible = true;
    }

    private int GetPasswordStrength(string pw)
    {
        int score = 0;
        if (pw.Length >= 8)  score += 25;
        if (pw.Length >= 12) score += 15;
        if (pw.Any(char.IsUpper))              score += 20;
        if (pw.Any(char.IsDigit))              score += 20;
        if (pw.Any(c => !char.IsLetterOrDigit(c))) score += 20;
        return Math.Min(score, 100);
    }
}