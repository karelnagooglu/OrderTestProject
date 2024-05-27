namespace OrderApi.Models
{
    public class Order
    {
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
