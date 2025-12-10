using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantManagement
{
    //    Example data

    //10:00 — Orders: 5, Deliveries: 0  ----->  5 pending
    //10:05 — Orders: 3, Deliveries: 2 -----> 6  pending
    //10:10 — Orders: 1, Deliveries: 4 -----> 3 pending
    //10:15 — Orders: 0, Deliveries: 2 -----> 1 pending
    //10:20 — Orders: 2, Deliveries: 1-----> 2 pending
    //What each spark chart shows(Y values)

    //Orders(spark line)
    //Y per bucket = items ordered in that bucket
    //Sequence: 5, 3, 1, 0, 2


    //Deliveries(spark column)
    //Y per bucket = items delivered in that bucket
    //Sequence: 0, 2, 4, 2, 1

    //Kitchen Load(spark area)
    //Y per bucket = active pending items = sum(orders in last 30 min) − sum(deliveries in last 30 min)
    //Up to 10:20 all points are within 30 minutes, so it’s the running backlog:
    //10:00: 5 − 0 = 5
    //10:05: (5+3) − (0+2) = 6
    //10:10: (5+3+1) − (0+2+4) = 3
    //10:15: (5+3+1+0) − (0+2+4+2) = 1
    //10:20: (5+3+1+0+2) − (0+2+4+2+1) = 2
    //Sequence: 5, 6, 3, 1, 2


    //Win/Loss(spark win/loss)
    //Y per step = sign of Kitchen Load change vs previous step
    //    If Kitchen Load increases → mark as Loss(−1)
    //    If Kitchen Load decreases → mark as Win(+1)
    //    If Kitchen Load stays same → mark as Neutral(0)
    //Sequence: 0, −1, +1, +1, −1 (first point neutral 0)

    public partial class DashboardViewModel : INotifyPropertyChanged
    {
        #region Fields

        // Constants and configuration
        private const string Filter1Hour = "Last 1 Hour";
        private const string Filter2Hours = "Last 2 Hours";
        private const string Filter4Hours = "Last 4 Hours";

        private static readonly TimeSpan DataStep = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan KitchenWindow = TimeSpan.FromMinutes(30);

        // Backing fields
        private double gradientStart;
        private double gradientEnd;
        private int ordersRowsCount;
        private int deliveriesRowsCount;
        private int pendingRowsCount;
        private string selectedFilter = Filter2Hours;

        // Random placeholder for sample metrics
        private readonly Random random = new();

        // Master data (unfiltered)
        private readonly ObservableCollection<OrderInfo> allOrdersData = new();
        private readonly ObservableCollection<DeliveryInfo> allDeliveriesData = new();
        private readonly ObservableCollection<KitchenLoadInfo> allKitchenLoadData = new();

        #endregion

        #region Properties

        public ObservableCollection<OrderInfo> OrdersData { get; private set; } = new();
        public ObservableCollection<KitchenLoadInfo> KitchenLoadData { get; private set; } = new();
        public ObservableCollection<DeliveryInfo> DeliveriesData { get; private set; } = new();
        public ObservableCollection<RevenueInfo> RevenueData { get; private set; } = new();
        public ObservableCollection<WinLossInfo> WinLossData { get; private set; } = new();

        public double GradientStart
        {
            get => gradientStart;
            private set { gradientStart = value; OnPropertyChanged(); }
        }

        public double GradientEnd
        {
            get => gradientEnd;
            private set { gradientEnd = value; OnPropertyChanged(); }
        }

        public int OrdersRowsCount
        {
            get => ordersRowsCount;
            private set { ordersRowsCount = value; OnPropertyChanged(); }
        }

        public int DeliveriesRowsCount
        {
            get => deliveriesRowsCount;
            private set { deliveriesRowsCount = value; OnPropertyChanged(); }
        }

        public int PendingRowsCount
        {
            get => pendingRowsCount;
            private set { pendingRowsCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> TimeFilters { get; } = new()
        {
            Filter1Hour, Filter2Hours, Filter4Hours
        };

        public string SelectedFilter
        {
            get => selectedFilter;
            set
            {
                if (selectedFilter == value) return;
                selectedFilter = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        #endregion

        #region Constructor

        public DashboardViewModel()
        {
            GenerateBaseData();
            BuildKitchenLoadFromAllData();

            // default filter
            SelectedFilter = Filter2Hours;
            ApplyFilter();
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}