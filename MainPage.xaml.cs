using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
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
        int allowedDistanceMeters = 100;
        CancellationTokenSource cancellationTokenReminder;

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
                    cancellationTokenReminder = new CancellationTokenSource();
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
               // MainContentPage.BackgroundColor = Colors.White;
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
                    int distanceToHomeM = (int) (Location.CalculateDistance(startLocation, curLocation, DistanceUnits.Kilometers) * 1000);
                    if (distanceToHomeM < allowedDistanceMeters)
                    {
                        GeoCoordLabel.Text = $"Forgot something? You are {distanceToHomeM} only meters away.";
                        //int secondsToVibrate = 1;
                        //TimeSpan vibrationLength = TimeSpan.FromSeconds(secondsToVibrate);
                        //Vibration.Default.Vibrate(vibrationLength);

                        var localNotification = new NotificationRequest
                        {
                            CategoryType = NotificationCategoryType.Alarm,
                            Title = "Remind me!",
                            Description = "Have you forgotten something?",
                            Android = new AndroidOptions
                            {
                                VibrationPattern = new long[] { 0, 200, 0, 200, 0, 200, 0, 200 },
                            },
                        };

                        _ = LocalNotificationCenter.Current.Show(localNotification);
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

        }

        private void OnEntryCompleted(object sender, EventArgs e)
        {
            GeolocationEntryName.Text = $"";
            GeoCoordLabel.Text = $"Geolocation saved";
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

        private async Task<Location> GetCurrentLocation()
        {

            Location location = null;
            try
            {
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

            return location;
        }
    }
}