namespace fiap_order_service.Models
{
    public class ItemOrder
    {
        /// <summary>
        /// Gets or sets the unique identifier for the vehicle.
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the amount value.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the unit price of the item.
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Gets or sets the total price of the items, including any applicable taxes or discounts.
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// The model of the vehicle.
        /// </summary>
        public required string Model { get; set; }

        /// <summary>
        /// The brand of the vehicle.
        /// </summary>
        public required string Brand { get; set; }

        /// <summary>
        /// The color of the vehicle.
        /// </summary>
        public required string Color { get; set; }

        /// <summary>
        /// The year the vehicle was manufactured.
        /// </summary>
        public int Year { get; set; }
    }
}
