using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace KinectPlayerRadar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor;
        private List<int> _skeletonTrackingIds;
        const int PixelsPerMeter = 300;
        const string BaseMarkerName = "player";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (!KinectSensor.KinectSensors.Any())
            {
                throw new ApplicationException("no kinect sensor detected");
            }

            _sensor = KinectSensor.KinectSensors[0];
            
            _skeletonTrackingIds = new List<int>();
            
            _sensor.SkeletonStream.Enable();
            _sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);            
            _sensor.Start();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            var index = 0;

            foreach (Skeleton skeleton in skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                {
                    SetMarkerPosition(skeleton, index);
                }

                index++;
            }

            RemoveUnusedMarkers(skeletons);
        }

        private void RemoveUnusedMarkers(Skeleton[] skeletons)
        {
            _skeletonTrackingIds.ForEach(id =>
                {
                    if (!skeletons.Select(s => s.TrackingId).Contains(id))
                    {
                        var marker = canvas.FindName(BaseMarkerName + id) as Ellipse;
                        if (marker != null)
                        {
                            canvas.Children.Remove(marker);
                        }
                    }
                });
        }

        private void SetMarkerPosition(Skeleton skeleton, int index)
        {         
            System.Windows.Shapes.Ellipse marker;
            var canvasCenter = canvas.Width / 2;
            var top = skeleton.Position.Z * PixelsPerMeter;
            var left = (canvas.Width / 2) + (skeleton.Position.X * PixelsPerMeter);

            if (!_skeletonTrackingIds.Contains(skeleton.TrackingId))
            {
                marker = AddToCanvas(skeleton.TrackingId, index);
                _skeletonTrackingIds.Add(skeleton.TrackingId);
            }
            else
            {
                marker = canvas.FindName(BaseMarkerName + skeleton.TrackingId) as System.Windows.Shapes.Ellipse;
            }

            if (marker != null)
            {
                Canvas.SetTop(marker, (skeleton.Position.Z * PixelsPerMeter));
                Canvas.SetLeft(marker, canvasCenter + (skeleton.Position.X * PixelsPerMeter));
            }
        }

        private Ellipse AddToCanvas(int skeletonTrackingId, int skeletonIndex)
        {
            var ellipse = new System.Windows.Shapes.Ellipse();
            ellipse.Name = BaseMarkerName + skeletonTrackingId;
            ellipse.Fill = GetMarkerColor(skeletonIndex);
            ellipse.Width = 30;
            ellipse.Height = 30;
            
            canvas.Children.Add(ellipse);
            canvas.RegisterName(ellipse.Name, ellipse);
          
            return ellipse;
        }

        private Brush GetMarkerColor(int playerIndex)
        {
            if (playerIndex == 1) return Brushes.Blue;
            if (playerIndex == 2) return Brushes.Red;
            if (playerIndex == 3) return Brushes.Green;
            if (playerIndex == 4) return Brushes.Orange;
            if (playerIndex == 5) return Brushes.Purple;

            return Brushes.Turquoise;
        }
    }
}
