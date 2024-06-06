using System.Text.Json.Serialization;

namespace OrderApi.Models
{
    public record OrderItem
    {
        [JsonPropertyName("productId")]
        public required string ProductId { get; init; }

        [JsonPropertyName("quantity")]
        public required long Quantity { get; init; }
    }
}
