using System.Collections.ObjectModel;
using System.Data;

namespace RestaurantManagement
{
    public partial class DashboardViewModel
    {
        #region Methods

        private void GenerateBaseData()
        {
            DateTime end = DateTime.Now;
            DateTime start = end.AddHours(-3);

            // repeating small patterns to keep data realistic but deterministic
            int[] ordersPattern = { 4, 2, 3, 1, 0, 2, 3, 2, 1, 3, 4, 2 };
            int[] deliveriesPattern = { 1, 2, 2, 2, 1, 2, 3, 2, 2, 2, 3, 3 };

            int index = 0;
            for (DateTime t = start; t <= end; t = t.Add(DataStep))
            {
                int orders = ordersPattern[index % ordersPattern.Length];
                int deliveries = deliveriesPattern[index % deliveriesPattern.Length];

                allOrdersData.Add(new OrderInfo
                {
                    TimeStamp = t,
                    Orders = orders,
                    Channel = ComputeChannel(index),
                    Table = ComputeTable(index),
                    Amount = Math.Round(10 + (orders * 2.5), 2)
                });

                allDeliveriesData.Add(new DeliveryInfo
                {
                    TimeStamp = t,
                    Deliveries = deliveries
                });

                index++;
            }
        }

        private static string ComputeChannel(int index)
        {
            int mod = index % 3;
            if (mod == 0) return "Dine-in";
            if (mod == 1) return "Takeaway";
            return "Delivery";
        }

        private static string ComputeTable(int index)
        {
            int mod = index % 3;
            if (mod == 0)
            {
                int tableNo = (index % 20) + 1;
                return $"T-{tableNo}";
            }

            int orderNo = 1000 + index;
            return $"O-{orderNo}";
        }

        private void BuildKitchenLoadFromAllData()
        {
            var allTimes = allOrdersData
                .Select(o => o.TimeStamp)
                .Union(allDeliveriesData.Select(d => d.TimeStamp))
                .OrderBy(t => t)
                .ToList();

            allKitchenLoadData.Clear();
            foreach (DateTime t in allTimes)
            {
                int pending = CalculatePendingAt(t);
                allKitchenLoadData.Add(new KitchenLoadInfo { TimeStamp = t, Load = pending });
            }
        }

        private void ApplyFilter()
        {
            // 1) time range
            (DateTime start, DateTime end) = ComputeTimeRangeForSelectedFilter();

            // 2) gradients for UI
            UpdateGradientForFilter();

            // 3) filter raw series
            var filteredOrders = FilterOrdersByRange(start, end);
            var filteredDeliveries = FilterDeliveriesByRange(start, end);

            ReplaceItems(OrdersData, filteredOrders);
            ReplaceItems(DeliveriesData, filteredDeliveries);

            // 4) kitchen load in window for the filtered time points
            var filteredLoad = BuildKitchenLoadFor(filteredOrders, filteredDeliveries);
            ReplaceItems(KitchenLoadData, filteredLoad);

            // 5) win/loss from load deltas
            var winLoss = BuildWinLossFromLoad(filteredLoad);
            ReplaceItems(WinLossData, winLoss);

            // 6) summary counters
            UpdateRowCounts(filteredOrders.Count);
        }

        private (DateTime start, DateTime end) ComputeTimeRangeForSelectedFilter()
        {
            DateTime end = DateTime.Now;
            DateTime start;

            if (SelectedFilter == Filter1Hour)
            {
                start = end.AddHours(-1);
            }
            else if (SelectedFilter == Filter2Hours)
            {
                start = end.AddHours(-2);
            }
            else
            {
                start = end.AddHours(-4);
            }

            return (start, end);
        }

        private void UpdateGradientForFilter()
        {
            if (SelectedFilter == Filter1Hour)
            {
                GradientStart = 0;
                GradientEnd = 5;
                return;
            }

            if (SelectedFilter == Filter2Hours)
            {
                GradientStart = 0;
                GradientEnd = 12;
                return;
            }

            GradientStart = 0;
            GradientEnd = 34;
        }

        private List<OrderInfo> FilterOrdersByRange(DateTime start, DateTime end)
        {
            return allOrdersData
                .Where(o => o.TimeStamp >= start && o.TimeStamp <= end)
                .OrderBy(o => o.TimeStamp)
                .ToList();
        }

        private List<DeliveryInfo> FilterDeliveriesByRange(DateTime start, DateTime end)
        {
            return allDeliveriesData
                .Where(d => d.TimeStamp >= start && d.TimeStamp <= end)
                .OrderBy(d => d.TimeStamp)
                .ToList();
        }

        private List<KitchenLoadInfo> BuildKitchenLoadFor(IEnumerable<OrderInfo> orders,
                                                          IEnumerable<DeliveryInfo> deliveries)
        {
            var times = orders.Select(o => o.TimeStamp)
                              .Union(deliveries.Select(d => d.TimeStamp))
                              .OrderBy(t => t)
                              .ToList();

            var loadPoints = new List<KitchenLoadInfo>(times.Count);
            foreach (DateTime t in times)
            {
                int pending = CalculatePendingAt(t);
                loadPoints.Add(new KitchenLoadInfo { TimeStamp = t, Load = pending });
            }

            return loadPoints;
        }

        private int CalculatePendingAt(DateTime t)
        {
            DateTime windowStart = t - KitchenWindow;

            int ordersInWindow = allOrdersData
                .Where(o => o.TimeStamp > windowStart && o.TimeStamp <= t)
                .Sum(o => o.Orders);

            int deliveriesInWindow = allDeliveriesData
                .Where(d => d.TimeStamp > windowStart && d.TimeStamp <= t)
                .Sum(d => d.Deliveries);

            int pending = ordersInWindow - deliveriesInWindow;
            return pending < 0 ? 0 : pending;
        }

        private static List<WinLossInfo> BuildWinLossFromLoad(IReadOnlyList<KitchenLoadInfo> load)
        {
            var series = new List<WinLossInfo>(load.Count);
            for (int i = 0; i < load.Count; i++)
            {
                if (i == 0)
                {
                    series.Add(new WinLossInfo { Value = 0 });
                    continue;
                }

                int delta = load[i].Load - load[i - 1].Load;
                int value;
                if (delta < 0) value = 1;       // improving
                else if (delta > 0) value = -1; // worsening
                else value = 0;                 // flat

                series.Add(new WinLossInfo { Value = value });
            }

            return series;
        }

        private void UpdateRowCounts(int filteredOrdersCount)
        {
            OrdersRowsCount = filteredOrdersCount;
            PendingRowsCount = random.Next(2, 5);
            DeliveriesRowsCount = Math.Max(0, OrdersRowsCount - PendingRowsCount);
        }

        private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> items)
        {
            target.Clear();
            foreach (var item in items)
            {
                target.Add(item);
            }
        }

        #endregion
    }
}