namespace RestaurantManagement
{
    public class OrderInfo
    {
        public DateTime TimeStamp { get; set; }
        public int Orders { get; set; }
        // Additional fields for list view details
        public string Channel { get; set; } = string.Empty;
        public string Table { get; set; } = string.Empty;
        public double Amount { get; set; }

        public string Reference
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Table)) return "-";
                if (Table.StartsWith("T-", StringComparison.OrdinalIgnoreCase))
                    return $"Table {Table.Substring(2)}";
                if (Table.StartsWith("O-", StringComparison.OrdinalIgnoreCase))
                    return $"Order #{Table.Substring(2)}";
                return Table;
            }
        }
    }

    public class KitchenLoadInfo
    {
        public DateTime TimeStamp { get; set; }
        public int Load { get; set; }
    }

    public class DeliveryInfo
    {
        public DateTime TimeStamp { get; set; }
        public int Deliveries { get; set; }
    }

    public class RevenueInfo
    {
        public string Month { get; set; } = string.Empty;
        public double Revenue { get; set; }
    }

    public class WinLossInfo
    {
        public int Value { get; set; }
    }
}