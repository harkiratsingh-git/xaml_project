using Postgrest.Attributes;
using Postgrest.Models;

namespace EventSpotter.Models;

public class CityData
{
    public string City { get; set; }
    public string Country { get; set; }
}

public class CategoryData
{
    public string Id { get; set; }
    public string Icon { get; set; }
    public string Label { get; set; }
    public string Desc { get; set; }
    public bool IsSelected { get; set; }
}

// ← ADD — maps to your Supabase profiles table
[Table("profiles")]
public class UserProfile : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("full_name")]
    public string FullName { get; set; }

    [Column("home_city_id")]
    public int? HomeCityId { get; set; }

    [Column("preferences")]
    public string[] Preferences { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Helper — not a DB column
    public string HomeCityName { get; set; }
}

// ← ADD — maps to your Supabase bookmark table
[Table("bookmark")]
public class BookmarkModel : BaseModel
{
    [PrimaryKey("bookmark_id", false)]
    public int BookmarkId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

// ← ADD — maps to your Supabase cart table
[Table("cart")]
public class CartModel : BaseModel
{
    [PrimaryKey("cart_id", false)]
    public int CartId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("added_at")]
    public DateTime AddedAt { get; set; }

    // Helper — not a DB column
    public EventModel Event { get; set; }
}

// ← ADD — maps to your Supabase review table
[Table("review")]
public class ReviewModel : BaseModel
{
    [PrimaryKey("review_id", false)]
    public int ReviewId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("event_id")]
    public int EventId { get; set; }

    [Column("rating")]
    public int Rating { get; set; }

    [Column("comment")]
    public string Comment { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}