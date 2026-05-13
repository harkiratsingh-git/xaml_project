using Postgrest.Attributes;
using Postgrest.Models;

namespace EventSpotter.Models;

[Table("event")]
public class EventModel : BaseModel
{
    [PrimaryKey("event_id", false)]
    public int Id { get; set; }

    [Column("event_title")]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("start_datetime")]
    public DateTime StartDateTime { get; set; }

    [Column("end_datetime")]
    public DateTime? EndDateTime { get; set; }

    [Column("source_url")]
    public string SourceUrl { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }

    [Column("is_free")]
    public bool IsFree { get; set; }

    [Column("venue_id")]
    public int? VenueId { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // ── New columns from your schema ─────────────────────────

    [Column("exclusive_performer")]
    public string ExclusivePerformer { get; set; }

    [Column("total_tickets")]
    public int TotalTickets { get; set; } = 100;

    [Column("sold_tickets")]
    public int SoldTickets { get; set; } = 0;

    [Column("is_priority")]
    public bool IsPriority { get; set; }

    [Column("price_eur")]
    public decimal? PriceEur { get; set; }

    // ── Helper properties — not mapped to DB ─────────────────
    public string CategoryName { get; set; }
    public string VenueName { get; set; }
    public string CityName { get; set; }
    public string CountryName { get; set; }
    public string CategoryColorHex { get; set; }

    // ── Computed helpers ──────────────────────────────────────
    public string FormattedDate =>
        StartDateTime.ToString("dd MMM yyyy · HH:mm");

    public bool IsUpcoming =>
        StartDateTime > DateTime.UtcNow;

    public int TicketsRemaining =>
        Math.Max(0, TotalTickets - SoldTickets);

    public double AvailabilityPercent =>
        TotalTickets > 0
            ? (double)SoldTickets / TotalTickets
            : 0;

    public bool IsAlmostSoldOut =>
        AvailabilityPercent >= 0.88;

    public string DisplayPrice =>
        IsFree ? "FREE"
        : PriceEur.HasValue ? $"€ {PriceEur.Value:F2}"
        : "€ 24.99";
}