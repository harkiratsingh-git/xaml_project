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

    public string HomeCityName { get; set; }
}

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

    public EventModel Event { get; set; }
}

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

// ── New models for venue and category ────────────────────────

[Table("venue")]
public class VenueModel : BaseModel
{
    [PrimaryKey("venue_id", false)]
    public int VenueId { get; set; }

    [Column("venue_name")]
    public string VenueName { get; set; }

    [Column("address")]
    public string Address { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }

    [Column("city_id")]
    public int? CityId { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }

    [Column("website")]
    public string Website { get; set; }

    [Column("phone")]
    public string Phone { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }
}

[Table("category")]
public class CategoryModel : BaseModel
{
    [PrimaryKey("category_id", false)]
    public int CategoryId { get; set; }

    [Column("category_name")]
    public string CategoryName { get; set; }

    [Column("icon")]
    public string Icon { get; set; }

    [Column("slug")]
    public string Slug { get; set; }
}

[Table("city")]
public class CityModel : BaseModel
{
    [PrimaryKey("city_id", false)]
    public int CityId { get; set; }

    [Column("city_name")]
    public string CityName { get; set; }

    [Column("country_id")]
    public int? CountryId { get; set; }
}

[Table("country")]
public class CountryModel : BaseModel
{
    [PrimaryKey("country_id", false)]
    public int CountryId { get; set; }

    [Column("country_name")]
    public string CountryName { get; set; }

    [Column("postcode")]
    public string Postcode { get; set; }
}