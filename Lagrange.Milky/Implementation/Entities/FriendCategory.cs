using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entities;

public class FriendCategory
{
    [JsonPropertyName("category_id")]
    public required int CategoryId { get; init; }

    [JsonPropertyName("category_name")]
    public required string CategoryName { get; init; }
}