using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Xml.Dom;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LocationTraking
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Geolocator locator;
        private ObservableCollection<string> coordinates = new ObservableCollection<string>();
        private ExtendedExecutionSession session;

        public MainPage()
        {
            this.InitializeComponent();
            locator = new Geolocator();
            locator.DesiredAccuracy = PositionAccuracy.High;
            locator.DesiredAccuracyInMeters = 0;
            locator.MovementThreshold = 1000;
            locator.ReportInterval = 5000;
            locator.PositionChanged += Locator_PositionChanged;
            coords.ItemsSource = coordinates;
            
            //Keep Running when Minimized 
            StartLocationExtensionSession();
        }
        private void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var coord = args.Position;
            string position = string.Format("{0},{1}",
                args.Position.Coordinate.Point.Position.Latitude, 
                args.Position.Coordinate.Point.Position.Longitude);
            var _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                coordinates.Insert(0, position);
            });

            string xml = $@"
            <toast activationType='foreground' launch='args'>
                <visual>
                    <binding template='ToastGeneric'>
                        <text>This is a toast notification</text>
                        <text>Latitude: {args.Position.Coordinate.Point.Position.Latitude} - Longitude: {args.Position.Coordinate.Point.Position.Longitude}</text>
                    </binding>
                </visual>
            </toast>";

            Windows.Data.Xml.Dom.XmlDocument doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(xml);

            ToastNotification notification = new ToastNotification(doc);
            ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
            notifier.Show(notification);
        }
        private async void StartLocationExtensionSession()
        {
            session = new ExtendedExecutionSession();
            session.Description = "Location Tracker";
            session.Reason = ExtendedExecutionReason.LocationTracking;
            session.Revoked += ExtendedExecutionSession_Revoked;
            var result = await session.RequestExtensionAsync();
            if (result == ExtendedExecutionResult.Denied)
            {
            }
        }

        private void ExtendedExecutionSession_Revoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            //TODO: clean up session data
            StopLocationExtensionSession();
        }

        private void StopLocationExtensionSession()
        {
            if (session != null)

            {
                session.Dispose();
                session = null;

            }
        }
    }
}
