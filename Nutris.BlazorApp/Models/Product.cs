namespace NutrisBlazor.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsNew { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Propiedades adicionales que podrías necesitar
        public decimal? DiscountPercentage { get; set; }
        public decimal PriceWithDiscount =>
            DiscountPercentage.HasValue
                ? Price * (1 - DiscountPercentage.Value / 100)
                : Price;
    }
}