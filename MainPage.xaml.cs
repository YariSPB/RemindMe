﻿//using Android.Gms.Tasks;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading;

namespace RemindMe
{
    public partial class MainPage : ContentPage
    {
        IBackgroundGPSService backgroundGPSService;
        bool reminderworkStarted = false;
       // int checkFrequency = 10;
        Location startLocation = null;
        int allowedDistanceMeters = 100;
        System.Threading.CancellationTokenSource cancellationTokenReminder;
        Color defaultBtnBackgroundColor;
        Color defaultpageContentBackgroundColor;
        System.Threading.CancellationTokenSource _cancelTokenSource;
        CancellationToken _cancellationToken;

        public MainPage(IBackgroundGPSService backgroundGPSService)
        {
            InitializeComponent();
            this.backgroundGPSService = backgroundGPSService;
            defaultBtnBackgroundColor = StartReminderBtn.BackgroundColor;
            defaultpageContentBackgroundColor = BackgroundColor;

            if(DeviceInfo.Platform == DevicePlatform.Android)
            {
                MessagingCenter.Subscribe<Location>(this, "Location", message =>
                {
                    if (_cancellationToken.IsCancellationRequested) {
                        return;
                    }
                       // fdfdfdf;
                    MainThread.BeginInvokeOnMainThread(() => {
                        int distanceToHomeM = (int)(Location.CalculateDistance(startLocation, message, DistanceUnits.Kilometers) * 1000);

                        if (distanceToHomeM < allowedDistanceMeters)
                        {
                            // too close
                            GeoCoordLabel.Text = $"Gentle reminder because you are close to home ({distanceToHomeM} meters).";
                            StartReminderBtn.TextColor = Colors.Red;
                            StartReminderBtn.BackgroundColor = Colors.White;
                            BackgroundColor = Colors.IndianRed;
                            
                            var localNotification = new NotificationRequest
                            {
                                CategoryType = NotificationCategoryType.Reminder,
                                Title = "Remind me!",
                                Description = "Forgotten something?",
                            };

                            LocalNotificationCenter.Current.Show(localNotification);
                            //  LocalNotificationCenter.Current.
                        }
                        else
                        {
                            // too far
                            StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                            StartReminderBtn.TextColor = Colors.White;
                            StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                            BackgroundColor = defaultpageContentBackgroundColor;
                            GeoCoordLabel.Text = $"Far from home: {distanceToHomeM} meters";
                        }
                    });
                });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message =>
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    MainThread.BeginInvokeOnMainThread(() => {
                        Debug.WriteLine("Location error");
                        GeoCoordLabel.Text = $"Location error";
                    });
                });
            }
        }

        //Run Remind me
        private async void OnStartReminderClicked(object sender, EventArgs e)
        {
            if (!StartReminderBtn.IsEnabled)
            {
                return;
            }

            StartReminderBtn.IsEnabled = false;
            StartReminderBtn.BackgroundColor = Colors.Gray;
            StartReminderBtn.Text = "Wait...";
            if (!reminderworkStarted)
            {
                startLocation = await GetCurrentLocation();
                if (startLocation != null && !startLocation.IsFromMockProvider)
                {
                    reminderworkStarted = true;
                    StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                    StartReminderBtn.Text = $"Stop";
                    GeoCoordLabel.Text = $"Will remind when closer than {allowedDistanceMeters} meters from home";
                    //GeoCoordLabel.Text = $"Latitude: {startLocation.Latitude}, Longitude: {startLocation.Longitude}";
                    cancellationTokenReminder = new System.Threading.CancellationTokenSource();
                    _cancellationToken = cancellationTokenReminder.Token;
                    // startfore
                    backgroundGPSService.Start();
                    //_ = PeriodicCheckHomeAsync(TimeSpan.FromSeconds(checkFrequency), cancellationTokenReminder.Token);
                }
                else
                {
                    GeoCoordLabel.Text = $"Failed collecting geolocation. Try again.";
                }
            }
            else
            {
                reminderworkStarted = false;
                // comment next
                cancellationTokenReminder.Cancel();
                backgroundGPSService.Stop();
                StartReminderBtn.Text = $"Start";
                StartReminderBtn.BackgroundColor = defaultBtnBackgroundColor;
                BackgroundColor = defaultpageContentBackgroundColor;
                GeoCoordLabel.Text = $"Finished. Shall we start again?";
                StartReminderBtn.TextColor = Colors.White;
            }

            StartReminderBtn.IsEnabled = true;
        }

        // Remind me based on current GPS location
        private async System.Threading.Tasks.Task PeriodicCheckHomeAsync(TimeSpan interval, System.Threading.CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                Location curLocation = await GetCurrentLocation();
                if(!cancellationToken.IsCancellationRequested && curLocation != null && !curLocation.IsFromMockProvider)
                {
                    int distanceToHomeM = (int) (Location.CalculateDistance(startLocation, curLocation, DistanceUnits.Kilometers) * 1000);
 
                    if (distanceToHomeM < allowedDistanceMeters)
                    {
                        // too close
                        GeoCoordLabel.Text = $"Gentle reminder because you are close to home ({distanceToHomeM} meters).";
                        //int secondsToVibrate = 1;
                        //TimeSpan vibrationLength = TimeSpan.FromSeconds(secondsToVibrate);
                        //Vibration.Default.Vibrate(vibrationLength);
                        StartReminderBtn.TextColor = Colors.Red;
                        StartReminderBtn.BackgroundColor = Colors.White;
                        BackgroundColor = Colors.IndianRed;
                        var localNotification = new NotificationRequest
                        {
                            CategoryType = NotificationCategoryType.Reminder,
                            Title = "Remind me!",
                            Description = "Forgotten something?",
                        };

                        await LocalNotificationCenter.Current.Show(localNotification);
                      //  LocalNotificationCenter.Current.
                    }
                    else
                    {
                        // too far
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