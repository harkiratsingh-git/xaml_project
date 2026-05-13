using Supabase;
using System.Globalization;

class Program
{
    // ── Your Supabase credentials ─────────────────────────────
    private const string SupabaseUrl = "https://cgiostxmsiczlpzwuhiw.supabase.co";
    private const string SupabaseKey = "sb_publishable_zlD_MLdb6PoQSGUJgbK-Ig_8hwsvQtt";

    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║   EVENT SPOTTER — CSV UPLOADER       ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();

        // 1. Find CSV file
        string csvPath = args.Length > 0 ? args[0] : "events.csv";

        if (!File.Exists(csvPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ CSV file not found: {csvPath}");
            Console.WriteLine("  Place events.csv next to this program or pass path as argument.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"✓ Found CSV: {csvPath}");

        // 2. Initialize Supabase
        Console.WriteLine("→ Connecting to Supabase...");
        var options = new SupabaseOptions { AutoConnectRealtime = false };
        var client = new Supabase.Client(SupabaseUrl, SupabaseKey, options);
        await client.InitializeAsync();
        Console.WriteLine("✓ Connected to Supabase");
        Console.WriteLine();

        // 3. Parse CSV
        var events = ParseCsv(csvPath);
        Console.WriteLine($"→ Parsed {events.Count} events from CSV");
        Console.WriteLine();

        // 4. Ask user what to do
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  [1] Insert all events (skip duplicates by title)");
        Console.WriteLine("  [2] Clear ALL events then insert fresh");
        Console.WriteLine("  [3] Preview only — don't upload");
        Console.Write("Choose [1/2/3]: ");
        var choice = Console.ReadLine()?.Trim();

        Console.WriteLine();

        switch (choice)
        {
            case "1":
                await InsertEvents(client, events, skipDuplicates: true);
                break;
            case "2":
                await ClearAndInsert(client, events);
                break;
            case "3":
                PreviewEvents(events);
                break;
            default:
                Console.WriteLine("Invalid choice. Exiting.");
                break;
        }

        Console.WriteLine();
        Console.WriteLine("Done. Press any key to exit.");
        Console.ReadKey();
    }

    // ── CSV PARSER ────────────────────────────────────────────
    static List<EventRow> ParseCsv(string path)
    {
        var events = new List<EventRow>();
        var lines = File.ReadAllLines(path);

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var cols = SplitCsvLine(line);
            if (cols.Length < 14)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠ Skipping line {i + 1} — not enough columns ({cols.Length}/14)");
                Console.ResetColor();
                continue;
            }

            try
            {
                var ev = new EventRow
                {
                    EventTitle         = cols[0].Trim(),
                    Description        = cols[1].Trim(),
                    StartDatetime      = DateTime.Parse(cols[2].Trim(), CultureInfo.InvariantCulture),
                    EndDatetime        = string.IsNullOrEmpty(cols[3].Trim()) ? null : DateTime.Parse(cols[3].Trim(), CultureInfo.InvariantCulture),
                    SourceUrl          = cols[4].Trim(),
                    ImageUrl           = cols[5].Trim(),
                    IsFree             = cols[6].Trim().ToLower() == "true",
                    PriceEur           = string.IsNullOrEmpty(cols[7].Trim()) ? null : decimal.Parse(cols[7].Trim(), CultureInfo.InvariantCulture),
                    ExclusivePerformer = cols[8].Trim(),
                    TotalTickets       = int.Parse(cols[9].Trim()),
                    SoldTickets        = int.Parse(cols[10].Trim()),
                    IsPriority         = cols[11].Trim().ToLower() == "true",
                    VenueId            = int.Parse(cols[12].Trim()),
                    CategoryId         = int.Parse(cols[13].Trim()),
                };
                events.Add(ev);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  ⚠ Skipping line {i + 1} — parse error: {ex.Message}");
                Console.ResetColor();
            }
        }

        return events;
    }

    // Handles quoted CSV fields with commas inside
    static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }

    // ── PREVIEW ───────────────────────────────────────────────
    static void PreviewEvents(List<EventRow> events)
    {
        Console.WriteLine("PREVIEW — events that would be uploaded:");
        Console.WriteLine(new string('─', 60));
        foreach (var ev in events)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  {ev.EventTitle}");
            Console.ResetColor();
            Console.WriteLine($"  [{ev.StartDatetime:dd MMM yyyy}]");
            Console.WriteLine($"    Price: {(ev.IsFree ? "FREE" : $"€{ev.PriceEur:F2}")}  " +
                              $"Tickets: {ev.TotalTickets}  " +
                              $"Priority: {ev.IsPriority}");
            if (!string.IsNullOrEmpty(ev.ExclusivePerformer))
                Console.WriteLine($"    ⭐ Exclusive: {ev.ExclusivePerformer}");
            Console.WriteLine();
        }
        Console.WriteLine($"Total: {events.Count} events");
    }

    // ── INSERT — skip duplicates ──────────────────────────────
    static async Task InsertEvents(Supabase.Client client,
        List<EventRow> events, bool skipDuplicates)
    {
        // Get existing titles
        var existing = new HashSet<string>();
        if (skipDuplicates)
        {
            Console.WriteLine("→ Checking for existing events...");
            var existingResponse = await client
                .From<SupabaseEvent>()
                .Select("event_title")
                .Get();
            foreach (var e in existingResponse.Models)
                existing.Add(e.EventTitle ?? "");
            Console.WriteLine($"  Found {existing.Count} existing events");
        }

        int inserted = 0;
        int skipped  = 0;
        int failed   = 0;

        foreach (var ev in events)
        {
            if (skipDuplicates && existing.Contains(ev.EventTitle))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  → SKIP (exists): {ev.EventTitle}");
                Console.ResetColor();
                skipped++;
                continue;
            }

            try
            {
                await client.From<SupabaseEvent>().Insert(new SupabaseEvent
                {
                    EventTitle         = ev.EventTitle,
                    Description        = ev.Description,
                    StartDatetime      = ev.StartDatetime,
                    EndDatetime        = ev.EndDatetime,
                    SourceUrl          = ev.SourceUrl,
                    ImageUrl           = ev.ImageUrl,
                    IsFree             = ev.IsFree,
                    PriceEur           = ev.PriceEur,
                    ExclusivePerformer = string.IsNullOrEmpty(ev.ExclusivePerformer)
                                            ? null : ev.ExclusivePerformer,
                    TotalTickets       = ev.TotalTickets,
                    SoldTickets        = ev.SoldTickets,
                    IsPriority         = ev.IsPriority,
                    VenueId            = ev.VenueId,
                    CategoryId         = ev.CategoryId,
                });

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"  ✓ Inserted: {ev.EventTitle}");
                Console.ResetColor();
                inserted++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  ✗ Failed:   {ev.EventTitle}");
                Console.WriteLine($"    Error: {ex.Message}");
                Console.ResetColor();
                failed++;
            }
        }

        Console.WriteLine();
        Console.WriteLine("─────────────────────────────────────");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Inserted : {inserted}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  → Skipped  : {skipped}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ Failed   : {failed}");
        Console.ResetColor();
    }

    // ── CLEAR AND INSERT ──────────────────────────────────────
    static async Task ClearAndInsert(Supabase.Client client,
        List<EventRow> events)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("⚠ WARNING: This will DELETE all existing events.");
        Console.ResetColor();
        Console.Write("Type YES to confirm: ");
        var confirm = Console.ReadLine()?.Trim();

        if (confirm != "YES")
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        Console.WriteLine("→ Deleting all existing events...");
        try
        {
            // Delete all rows — requires admin key
            await client.From<SupabaseEvent>()
                .Filter("event_id", Postgrest.Constants.Operator.GreaterThan, "0")
                .Delete();
            Console.WriteLine("✓ All events deleted");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ Delete failed: {ex.Message}");
            Console.ResetColor();
            return;
        }

        await InsertEvents(client, events, skipDuplicates: false);
    }
}

// ── LOCAL CSV ROW MODEL ───────────────────────────────────────
class EventRow
{
    public string   EventTitle         { get; set; }
    public string   Description        { get; set; }
    public DateTime StartDatetime      { get; set; }
    public DateTime? EndDatetime       { get; set; }
    public string   SourceUrl          { get; set; }
    public string   ImageUrl           { get; set; }
    public bool     IsFree             { get; set; }
    public decimal? PriceEur           { get; set; }
    public string   ExclusivePerformer { get; set; }
    public int      TotalTickets       { get; set; }
    public int      SoldTickets        { get; set; }
    public bool     IsPriority         { get; set; }
    public int      VenueId            { get; set; }
    public int      CategoryId         { get; set; }
}

// ── SUPABASE MODEL ────────────────────────────────────────────
[Postgrest.Attributes.Table("event")]
class SupabaseEvent : Postgrest.Models.BaseModel
{
    [Postgrest.Attributes.PrimaryKey("event_id", false)]
    public int EventId { get; set; }

    [Postgrest.Attributes.Column("event_title")]
    public string EventTitle { get; set; }

    [Postgrest.Attributes.Column("description")]
    public string Description { get; set; }

    [Postgrest.Attributes.Column("start_datetime")]
    public DateTime StartDatetime { get; set; }

    [Postgrest.Attributes.Column("end_datetime")]
    public DateTime? EndDatetime { get; set; }

    [Postgrest.Attributes.Column("source_url")]
    public string SourceUrl { get; set; }

    [Postgrest.Attributes.Column("image_url")]
    public string ImageUrl { get; set; }

    [Postgrest.Attributes.Column("is_free")]
    public bool IsFree { get; set; }

    [Postgrest.Attributes.Column("price_eur")]
    public decimal? PriceEur { get; set; }

    [Postgrest.Attributes.Column("exclusive_performer")]
    public string ExclusivePerformer { get; set; }

    [Postgrest.Attributes.Column("total_tickets")]
    public int TotalTickets { get; set; }

    [Postgrest.Attributes.Column("sold_tickets")]
    public int SoldTickets { get; set; }

    [Postgrest.Attributes.Column("is_priority")]
    public bool IsPriority { get; set; }

    [Postgrest.Attributes.Column("venue_id")]
    public int VenueId { get; set; }

    [Postgrest.Attributes.Column("category_id")]
    public int CategoryId { get; set; }
}