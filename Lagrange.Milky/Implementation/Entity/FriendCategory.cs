using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity;

public class FriendCategory(int categoryId, string categoryName)
{
    [JsonPropertyName("category_id")]
    public int CategoryId { get; } = categoryId;

    [JsonPropertyName("category_name")]
    public string CategoryName { get; } = categoryName;
}