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

    // ← ADD end_datetime to match your Supabase schema
    [Column("end_datetime")]
    public DateTime? EndDateTime { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }

    [Column("is_free")]
    public bool IsFree { get; set; }

    // ← ADD source_url to match your Supabase schema
    [Column("source_url")]
    public string SourceUrl { get; set; }

    // ← ADD venue_id and category_id foreign keys
    [Column("venue_id")]
    public int VenueId { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    // Helper properties — NOT mapped to DB columns
    // These get filled manually after fetching
    public string CategoryName { get; set; }
    public string VenueName { get; set; }
    public string CityName { get; set; }
    public string CountryName { get; set; }

    // ← REMOVE Color CategoryColor — Color is a MAUI type
    // that Postgrest cannot serialize. Use a hex string instead
    public string CategoryColorHex { get; set; }

    // Computed helper for display
    public string FormattedDate =>
        StartDateTime.ToString("dd MMM yyyy · HH:mm");

    public bool IsUpcoming =>
        StartDateTime > DateTime.UtcNow;
}