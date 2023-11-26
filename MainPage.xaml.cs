using System.ComponentModel.Design;
using System.Threading;

namespace RemindMe
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        double targetLatitude = 42.099077;
        double targetLongitude = 19.093777;
        Location dummy = new Location(latitude: 42.099077, longitude: 19.093777);
        //target 42.099077, 19.093777
        //init 42.098414, 19.096313
        double initLatitude = 42.098414;
        double initLongitude = 19.096313;
        bool visitedAll = false;
        bool reminderworkStarted = false;
        Location startLocation = null;
        int allowedDistanceMeters = 30;
        CancellationTokenSource cancellationTokenReminder = new CancellationTokenSource();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnStartReminderClicked(object sender, EventArgs e)
        {
            if (!StartReminderBtn.IsEnabled)
            {
                return;
            }

            StartReminderBtn.IsEnabled = false;
            if (!reminderworkStarted)
            {
                startLocation = await GetCurrentLocation();
                if (startLocation != null && !startLocation.IsFromMockProvider)
                {
                    reminderworkStarted = true;
                    StartReminderBtn.Text = $"Stop reminder.";
                    StartReminderBtn.TextColor = Colors.Pink;
                    GeoCoordLabel.Text = $"Latitude: {startLocation.Latitude}, Longitude: {startLocation.Longitude}";
                    _ = PeriodicCheckHomeAsync(TimeSpan.FromSeconds(5), cancellationTokenReminder.Token);
                }
                else
                {
                    GeoCoordLabel.Text = $"Failed collecting geolocation. Try again.";
                }
            }
            else
            {
                reminderworkStarted = false;
                cancellationTokenReminder.Cancel();
                StartReminderBtn.Text = $"Begin reminding, please!";
                GeoCoordLabel.Text = $"Finished. Shall we start again?";
                StartReminderBtn.TextColor = Colors.DarkBlue;
            }

            StartReminderBtn.IsEnabled = true;
        }

        private async Task PeriodicCheckHomeAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);

                Location curLocation = await GetCurrentLocation();
                if(curLocation != null && !curLocation.IsFromMockProvider)
                {
                    int distanceToHomeM = (int) (Location.CalculateDistance(dummy, curLocation, DistanceUnits.Kilometers) * 1000);
                    if (distanceToHomeM < allowedDistanceMeters)
                    {
                        GeoCoordLabel.Text = $"Close to home: {distanceToHomeM} meters.";
                    }
                    else
                    {
                        GeoCoordLabel.Text = $"Far from home: {distanceToHomeM} meters";
                    }
                }
            }
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            GeolocationEntryName.Text = $"";
            GeoCoordLabel.Text = $"Geolocation saved";

            // SemanticScreenReader.Announce(GeolocationEntryName.Text);
            // SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void RunReminder()
        {
            while (!visitedAll)
            {
              //  await FooAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

        }

        private async void OnGeoClicked(object sender, EventArgs e)
        {
            Location location = await GetCurrentLocation();
            if (location != null && !location.IsFromMockProvider)
                GeoCoordLabel.Text = $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}";
            else
                GeoCoordLabel.Text = $"Geolocation timeout";

            SemanticScreenReader.Announce(GeoCoordLabel.Text);
        }


        private CancellationTokenSource _cancelTokenSource;
        private bool _isCheckingLocation;

        private async Task<Location> GetCurrentLocation()
        {

            Location location = null;
            try
            {
                _isCheckingLocation = true;
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                _cancelTokenSource = new CancellationTokenSource();
                location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);
            }
            // Catch one of the following exceptions:
            //   FeatureNotSupportedException
            //   FeatureNotEnabledException
            //   PermissionException
            catch (Exception ex)
            {
            }
            finally
            {
                _isCheckingLocation = false;
            }

            return location;
        }

        public void CancelRequest()
        {
            if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
                _cancelTokenSource.Cancel();
        }

    }
}