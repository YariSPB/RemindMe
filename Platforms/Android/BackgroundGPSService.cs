using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace RemindMe
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeLocation)]
    public class BackgroundGPSService : Service, IBackgroundGPSService
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public const string notificationChannelId = "10099";
        private const int NOTIFICATION_ID = 10009;
        private const string notificationChannelName = "RemindMeNotification";
        private static Context context = global::Android.App.Application.Context;
        private CancellationTokenSource _cts;
       // private Context context = Android.App.Application.Context;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var intentPending= new Intent(context, typeof(MainActivity));
            // var pendingIntentFlags = Build.VERSION.SdkInt >= BuildVersionCodes.S
            //     ? PendingIntentFlags.UpdateCurrent |
            //       PendingIntentFlags.Mutable
            //     : PendingIntentFlags.UpdateCurrent;
            var pendingIntent = PendingIntent.GetActivity(Android.App.Application.Context, 0, intentPending, PendingIntentFlags.UpdateCurrent);
            var notification = new NotificationCompat.Builder(context, notificationChannelId)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(false)
                .SetOngoing(true)
               .SetSmallIcon(Resource.Drawable.mtrl_ic_check_mark)
                .SetContentTitle("My Aoo")
                .SetContentText("My App Service is running");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(notificationChannelId, notificationChannelName, NotificationImportance.High);
                channel.EnableVibration(true);

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager.CreateNotificationChannel(channel);
            }

            StartForeground(NOTIFICATION_ID, notification.Build());

            _cts = new CancellationTokenSource();

            //MainThread.BeginInvokeOnMainThread(() => {
            
            Task.Run(() =>
             {
            try
            {
                var locationService = new GetLocationService();
                locationService.Run(_cts.Token).Wait();
            }
            catch (Android.OS.OperationCanceledException)
            {
            }
            finally
            {

            }

            }, _cts.Token);
            return StartCommandResult.Sticky;
        }

        public void Start()
        {
           // var intent = new Intent(MainActivity, typeof(BackgroundGPSService));
             var intent = new Intent(Android.App.Application.Context, typeof(BackgroundGPSService));
            StartForegroundService(intent);
        }

        public void Stop()
        {
           // StopForeground(true);
           var intent = new Intent(Android.App.Application.Context, typeof(BackgroundGPSService));
           // StopService(intent);
        }

        public override void OnDestroy()
        {
            if(_cts != null)
            {
                _cts.Token.ThrowIfCancellationRequested();
                _cts.Cancel();
            }
            base.OnDestroy();
        }
    }
}