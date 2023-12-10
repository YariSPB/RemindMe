using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.ComponentModel.Design;
using System.Threading;

namespace RemindMe
{
    public partial class MainPage : ContentPage
    {
        bool reminderworkStarted = false;
        Location startLocation = null;
        int allowedDistanceMeters = 100;
        CancellationTokenSource cancellationTokenReminder;
        Color defaultBtnBackgroundColor;
        Color defaultpageContentBackgroundColor;
        CancellationTokenSource _cancelTokenSource;

        public MainPage()
        {
            InitializeComponent();
            defaultBtnBackgroundColor = StartReminderBtn.BackgroundColor;
            defaultpageContentBackgroundColor = BackgroundColor;
        }

        //Run Remind me
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
                    StartReminderBtn.Text = $"Stop RemindMe.";
                    GeoCoordLabel.Text = $"Home GPS location recorded. I will remind you within {allowedDistanceMeters} m distance";
                    //GeoCoordLabel.Text = $"Latitude: {startLocation.Latitude}, Longitude: {startLocation.Longitude}";
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
                StartReminderBtn.Text = $"Start reminding!";
                StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                BackgroundColor = defaultpageContentBackgroundColor;
                GeoCoordLabel.Text = $"Finished. Shall we start again?";
                StartReminderBtn.TextColor = Colors.White;
            }

            StartReminderBtn.IsEnabled = true;
        }

        // Remind me based on current GPS location
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
                        GeoCoordLabel.Text = $"Forgetting things? U are just {distanceToHomeM} meters from home.";
                        //int secondsToVibrate = 1;
                        //TimeSpan vibrationLength = TimeSpan.FromSeconds(secondsToVibrate);
                        //Vibration.Default.Vibrate(vibrationLength);
                        StartReminderBtn.TextColor = Colors.Red;
                        StartReminderBtn.BackgroundColor = Colors.White;
                        BackgroundColor = Colors.IndianRed;
                        var localNotification = new NotificationRequest
                        {
                            CategoryType = NotificationCategoryType.Alarm,
                            Title = "Remind me!",
                            Description = "Forgetting things?",
                            Android = new AndroidOptions
                            {
                                VibrationPattern = new long[] { 0, 200, 0, 200, 0, 200, 0, 200 },
                            },
                        };

                        _ = LocalNotificationCenter.Current.Show(localNotification);
                    }
                    else
                    {
                        StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                        StartReminderBtn.TextColor = Colors.White;
                        StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                        BackgroundColor = defaultpageContentBackgroundColor;
                        GeoCoordLabel.Text = $"Far from home: {distanceToHomeM} meters";
                    }
                }
            }
        }

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