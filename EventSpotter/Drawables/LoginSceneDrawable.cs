using Microsoft.Maui.Graphics;

namespace EventSpotter.Drawables;

public class LoginSceneDrawable : IDrawable
{
    public int EmailLength { get; set; }
    public int PasswordLength { get; set; }
    public bool IsBroken { get; set; }
    public bool LoginSuccess { get; set; }
    public string Mode { get; set; } = "login";

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        // Background grid
        canvas.StrokeColor = Color.FromRgba(0, 180, 255, 15);
        canvas.StrokeSize = 0.5f;
        for (int i = 0; i < dirtyRect.Width; i += 50)
            canvas.DrawLine(i, 0, i, dirtyRect.Height);
        for (int j = 0; j < dirtyRect.Height; j += 50)
            canvas.DrawLine(0, j, dirtyRect.Width, j);

        if (Mode == "login")
            DrawF1Scene(canvas, dirtyRect);
        else
            DrawRocketScene(canvas, dirtyRect);
    }

    // ── F1 ASSEMBLY SCENE (Login) ─────────────────────────────
    private void DrawF1Scene(ICanvas canvas, RectF rect)
    {
        float W = rect.Width;
        float H = rect.Height;
        float emailProg  = Math.Min(EmailLength  / 25f, 1f);
        float passProg   = Math.Min(PasswordLength / 12f, 1f);
        float assembled  = emailProg * 0.5f + passProg * 0.5f;

        // Ceiling lights
        for (int i = 1; i <= 4; i++)
        {
            float lx = W * (i / 5f);
            float alpha = 0.15f + emailProg * 0.4f;
            canvas.FillColor = Color.FromRgba(254, 249, 195, (int)(alpha * 255));
            canvas.FillRectangle(lx - 20, 0, 40, 6);
        }

        // Conveyor belt
        canvas.FillColor = Color.FromArgb("#1e293b");
        canvas.FillRectangle(0, H * 0.75f, W, 18);
        canvas.StrokeColor = Color.FromArgb("#334155");
        canvas.StrokeSize = 1;
        for (int x = 0; x < W; x += 40)
        {
            canvas.DrawLine(x, H * 0.75f, x + 20, H * 0.75f + 18);
        }

        // Gears — top left
        DrawGear(canvas, W * 0.08f, H * 0.25f, 40, 9,
            emailProg * 3, Color.FromArgb("#00d4ff"),
            0.2f + emailProg * 0.5f);
        DrawGear(canvas, W * 0.15f, H * 0.25f, 24, 6,
            -emailProg * 4.5f, Color.FromArgb("#ff6b35"),
            0.15f + emailProg * 0.4f);

        // Gears — top right
        DrawGear(canvas, W * 0.92f, H * 0.25f, 36, 8,
            passProg * 2.5f, Color.FromArgb("#b06cff"),
            0.2f + passProg * 0.5f);
        DrawGear(canvas, W * 0.85f, H * 0.25f, 22, 5,
            -passProg * 3.8f, Color.FromArgb("#00d4ff"),
            0.15f + passProg * 0.4f);

        // F1 car body
        float cx = W * 0.5f;
        float cy = H * 0.68f;
        float scale = assembled;

        if (scale > 0.1f)
        {
            // Shadow
            canvas.FillColor = Color.FromRgba(0, 0, 0, 60);
            canvas.FillEllipse(cx - 60 * scale, cy + 16, 120 * scale, 10);

            // Car body
            canvas.FillColor = IsBroken
                ? Color.FromArgb("#444444")
                : Color.FromArgb("#dc2626");

            var bodyPath = new PathF();
            bodyPath.MoveTo(cx - 65 * scale, cy + 6);
            bodyPath.LineTo(cx - 50 * scale, cy - 4);
            bodyPath.LineTo(cx - 20 * scale, cy - 8);
            bodyPath.LineTo(cx + 20 * scale, cy - 8);
            bodyPath.LineTo(cx + 50 * scale, cy - 4);
            bodyPath.LineTo(cx + 65 * scale, cy + 2);
            bodyPath.LineTo(cx + 65 * scale, cy + 8);
            bodyPath.LineTo(cx - 65 * scale, cy + 8);
            bodyPath.Close();
            canvas.FillPath(bodyPath);

            // Cockpit
            if (scale > 0.3f)
            {
                canvas.FillColor = IsBroken
                    ? Color.FromArgb("#222222")
                    : Color.FromArgb("#0f172a");
                var cockpit = new PathF();
                cockpit.MoveTo(cx - 18 * scale, cy - 8);
                cockpit.LineTo(cx - 8 * scale, cy - 22);
                cockpit.LineTo(cx + 8 * scale, cy - 22);
                cockpit.LineTo(cx + 18 * scale, cy - 8);
                cockpit.Close();
                canvas.FillPath(cockpit);

                // Visor
                canvas.FillColor = IsBroken
                    ? Color.FromArgb("#1a1a1a")
                    : Color.FromArgb("#7dd3fc");
                canvas.FillEllipse(cx - 7 * scale, cy - 20, 14 * scale, 9);
            }

            // Front wing
            if (scale > 0.5f)
            {
                canvas.FillColor = IsBroken
                    ? Color.FromArgb("#222222")
                    : Color.FromArgb("#111827");
                var fw = new PathF();
                fw.MoveTo(cx + 65 * scale, cy + 6);
                fw.LineTo(cx + 90 * scale, cy + 6);
                fw.LineTo(cx + 95 * scale, cy + 12);
                fw.LineTo(cx + 60 * scale, cy + 12);
                fw.Close();
                canvas.FillPath(fw);
            }

            // Rear wing
            if (scale > 0.5f)
            {
                canvas.FillColor = IsBroken
                    ? Color.FromArgb("#222222")
                    : Color.FromArgb("#111827");
                canvas.FillRectangle(cx - 75 * scale, cy - 28, 30 * scale, 5);
            }

            // Wheels
            if (scale > 0.2f)
            {
                float[] wpos = { -42, 38 };
                foreach (float wx in wpos)
                {
                    canvas.FillColor = IsBroken
                        ? Color.FromArgb("#1a1a1a")
                        : Color.FromArgb("#111827");
                    canvas.FillEllipse(cx + wx * scale - 11, cy + 8, 22, 22);
                    canvas.FillColor = Color.FromArgb("#6b7280");
                    canvas.FillEllipse(cx + wx * scale - 6, cy + 13, 12, 12);
                }
            }

            // Engine glow when login success
            if (LoginSuccess && !IsBroken)
            {
                canvas.FillColor = Color.FromRgba(255, 100, 0, 180);
                canvas.FillEllipse(cx - 75 * scale, cy - 2, 18, 10);
            }
        }

        // Smoke when broken
        if (IsBroken)
        {
            for (int s = 0; s < 5; s++)
            {
                float sx = cx - 50 + s * 8;
                float sy = cy - 20 - s * 12;
                float sa = (5 - s) * 40;
                canvas.FillColor = Color.FromRgba(80, 80, 80, (int)sa);
                canvas.FillEllipse(sx, sy, 20 + s * 5, 20 + s * 5);
            }
        }

        // Robot arm — appears when typing password
        if (passProg > 0.05f)
        {
            DrawRobotArm(canvas,
                W * 0.72f, H * 0.55f,
                -1.2f + passProg * 0.9f,
                passProg);
        }

        // Status text
        canvas.FontSize = 9;
        canvas.FontColor = Color.FromArgb("#00d4ff");
        if (emailProg > 0)
            canvas.DrawString(
                $"ASSEMBLY: {(int)(assembled * 100)}%",
                16, H - 40, 200, 20,
                HorizontalAlignment.Left,
                VerticalAlignment.Center);
        if (passProg > 0)
        {
            canvas.FontColor = IsBroken
                ? Color.FromArgb("#ff4d4d")
                : LoginSuccess
                    ? Color.FromArgb("#06d6a0")
                    : Color.FromArgb("#3a5068");
            canvas.DrawString(
                IsBroken ? "ENGINE FAILURE"
                    : LoginSuccess ? "ENGINE RUNNING"
                    : "CALIBRATING...",
                16, H - 24, 200, 20,
                HorizontalAlignment.Left,
                VerticalAlignment.Center);
        }
    }

    private void DrawGear(ICanvas canvas, float cx, float cy,
        float radius, int teeth, float angle, Color color, float alpha)
    {
        canvas.SaveState();
        canvas.Translate(cx, cy);
        canvas.Rotate(angle * 57.2958f);
        canvas.StrokeColor = Color.FromRgba(
            (int)(color.Red * 255),
            (int)(color.Green * 255),
            (int)(color.Blue * 255),
            (int)(alpha * 255));
        canvas.StrokeSize = 1.5f;

        var path = new PathF();
        for (int i = 0; i < teeth * 2; i++)
        {
            float a = (float)(i / (teeth * 2.0) * Math.PI * 2);
            float r = i % 2 == 0 ? radius : radius * 0.78f;
            float px = (float)Math.Cos(a) * r;
            float py = (float)Math.Sin(a) * r;
            if (i == 0) path.MoveTo(px, py);
            else path.LineTo(px, py);
        }
        path.Close();
        canvas.DrawPath(path);
        canvas.DrawCircle(0, 0, radius * 0.38f);
        canvas.RestoreState();
    }

    private void DrawRobotArm(ICanvas canvas, float bx, float by,
        float angle, float alpha)
    {
        canvas.SaveState();
        canvas.Translate(bx, by);

        // Base
        canvas.FillColor = Color.FromRgba(51, 65, 85, (int)(alpha * 200));
        canvas.FillRoundedRectangle(-14, -4, 28, 22, 3);

        // Upper arm
        canvas.SaveState();
        canvas.Rotate(angle * 57.2958f);
        canvas.FillColor = Color.FromRgba(71, 85, 105, (int)(alpha * 200));
        canvas.FillRoundedRectangle(-5, -80, 10, 80, 3);

        // Forearm
        canvas.Translate(0, -80);
        canvas.Rotate(28.6f);
        canvas.FillColor = Color.FromRgba(100, 116, 139, (int)(alpha * 200));
        canvas.FillRoundedRectangle(-4, -55, 8, 55, 2);

        // Tool tip
        canvas.Translate(0, -55);
        canvas.StrokeColor = Color.FromRgba(0, 212, 255, (int)(alpha * 200));
        canvas.StrokeSize = 2;
        canvas.DrawLine(-5, -4, 5, -4);
        canvas.DrawLine(0, -8, 0, 0);

        canvas.RestoreState();
        canvas.RestoreState();
    }

    // ── ROCKET SCENE (Register) ───────────────────────────────
    private void DrawRocketScene(ICanvas canvas, RectF rect)
    {
        float W = rect.Width;
        float H = rect.Height;
        float emailProg = Math.Min((EmailLength) / 30f, 1f);
        float passProg  = Math.Min(PasswordLength / 12f, 1f);

        // Launch platform
        canvas.FillColor = Color.FromRgba(30, 41, 59, 120);
        canvas.FillRectangle(W * 0.3f, H * 0.85f, W * 0.4f, 16);

        // Silo guides
        canvas.StrokeColor = Color.FromRgba(0, 212, 255, 30);
        canvas.StrokeSize = 1;
        canvas.DrawLine(W * 0.42f, H * 0.85f, W * 0.42f, H);
        canvas.DrawLine(W * 0.58f, H * 0.85f, W * 0.58f, H);

        // Rocket position
        float rocketY = H * 0.85f - emailProg * H * 0.45f;
        if (LoginSuccess)
            rocketY -= passProg * H * 0.6f;

        float rx = W * 0.5f;
        float ry = rocketY;

        // Flame
        if (passProg > 0.3f || LoginSuccess)
        {
            float flameH = LoginSuccess ? 80 : 20 + passProg * 50;
            canvas.FillColor = Color.FromRgba(255, 100, 0, 200);
            var flame = new PathF();
            flame.MoveTo(rx - 10, ry + 20);
            flame.LineTo(rx, ry + 20 + flameH);
            flame.LineTo(rx + 10, ry + 20);
            flame.Close();
            canvas.FillPath(flame);
        }

        // Rocket body
        canvas.FillColor = emailProg > 0.1f
            ? Color.FromArgb("#e2f4ff")
            : Color.FromArgb("#1e293b");
        canvas.FillRoundedRectangle(rx - 14, ry - 60, 28, 80, 4);

        // Nose cone
        var nose = new PathF();
        nose.MoveTo(rx - 14, ry - 60);
        nose.LineTo(rx, ry - 95);
        nose.LineTo(rx + 14, ry - 60);
        nose.Close();
        canvas.FillColor = Color.FromArgb("#7dd3fc");
        canvas.FillPath(nose);

        // Left fin
        var lFin = new PathF();
        lFin.MoveTo(rx - 14, ry);
        lFin.LineTo(rx - 34, ry + 28);
        lFin.LineTo(rx - 14, ry + 18);
        lFin.Close();
        canvas.FillColor = Color.FromArgb("#0ea5e9");
        canvas.FillPath(lFin);

        // Right fin
        var rFin = new PathF();
        rFin.MoveTo(rx + 14, ry);
        rFin.LineTo(rx + 34, ry + 28);
        rFin.LineTo(rx + 14, ry + 18);
        rFin.Close();
        canvas.FillPath(rFin);

        // Window
        canvas.FillColor = Color.FromArgb("#7dd3fc");
        canvas.FillEllipse(rx - 8, ry - 38, 16, 16);

        // Fuel arm
        if (emailProg > 0.5f)
        {
            float armEndX = W * 0.15f + passProg * (rx - W * 0.15f - 30);
            canvas.StrokeColor = Color.FromRgba(71, 85, 105, 200);
            canvas.StrokeSize = 5;
            canvas.DrawLine(W * 0.15f, H * 0.6f, armEndX, H * 0.6f);

            // Fuel level bar
            canvas.FillColor = Color.FromRgba(255, 255, 255, 15);
            canvas.FillRectangle(W * 0.15f, H * 0.6f + 14, 100, 5);
            var fuelColor = passProg < 0.4f
                ? Color.FromArgb("#ff6b35")
                : passProg < 0.75f
                    ? Color.FromArgb("#ffd166")
                    : Color.FromArgb("#06d6a0");
            canvas.FillColor = fuelColor;
            canvas.FillRectangle(W * 0.15f, H * 0.6f + 14, passProg * 100, 5);

            canvas.FontSize = 8;
            canvas.FontColor = fuelColor;
            canvas.DrawString(
                $"{(int)(passProg * 100)}% FUEL",
                (int)(W * 0.15f), (int)(H * 0.6f + 26),
                100, 16,
                HorizontalAlignment.Left,
                VerticalAlignment.Center);
        }

        // ES text on rocket
        canvas.FontSize = 8;
        canvas.FontColor = Color.FromArgb("#ffffff");
        canvas.DrawString("ES", rx - 8, ry - 15, 16, 12,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}