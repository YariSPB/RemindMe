using Microsoft.Maui.Devices.Sensors;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemindMe
{
    public class LocationMessage
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LocationErrorMessage
    {

    }

    public class GetLocationService
    {
        readonly bool stopping = false;
        int checkFrequency = 15;
        public GetLocationService() { }

        public async Task Run(CancellationToken token)
        {
            //MainThread.BeginInvokeOnMainThread(async () => {
                await Task.Run(async () => {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    try {
                        await Task.Delay(TimeSpan.FromSeconds(checkFrequency));
                        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best);
                        var curLocation = await Geolocation.GetLocationAsync(request);
                        if (curLocation != null && !curLocation.IsFromMockProvider) {
                            var message = new LocationMessage
                            {
                                Latitude = curLocation.Latitude,
                                Longitude = curLocation.Longitude
                            };

                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                MessagingCenter.Send<Location>(curLocation, "Location");
                            });
                        }

                    } catch(Exception ex)
                    {
                        var errorMessage = new LocationErrorMessage();
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send(errorMessage, "LocationError");
                        });
                    }
                }
            }, token);   
        }      
    }
}
