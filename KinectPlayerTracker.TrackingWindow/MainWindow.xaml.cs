using System;
using System.ComponentModel;
using System.Windows;
using KinectPlayerTracker.Toolkit;
using Microsoft.Kinect;

namespace KinectPlayerTracker.TrackingWindow
{
    public partial class MainWindow : Window
    {
        private KinectSensorManager _sensorManager;
        private KinectSensorChooser _sensorChooser;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            IntializeTcpConnection();

            _sensorManager = new KinectSensorManager();
            _sensorManager.KinectSensorChanged += OnKinectSensorChanged;

            _sensorChooser = new KinectSensorChooser();
            _sensorChooser.KinectSensorChanged += OnKinectSensorChoosen;
            _sensorChooser.PropertyChanged += OnKinectSensorChooserPropertyChanged;
            _sensorChooser.Start();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {

        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            if (_sensorChooser != null)
            {
                _sensorChooser.Stop();
                _sensorChooser.KinectSensorChanged -= OnKinectSensorChoosen;
                _sensorChooser.PropertyChanged -= OnKinectSensorChooserPropertyChanged;
            }

            if (_sensorManager != null)
            {
                _sensorManager.KinectSensor = null;
                _sensorManager.KinectSensorChanged -= OnKinectSensorChanged;
            }
        }

        private void OnPlayFieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this._renderSize = new Size(this.playField.ActualWidth, this.playField.ActualHeight);
        }

        private void OnKinectSensorChoosen(object sender, KinectSensorChangedEventArgs<KinectSensor> e)
        {
            _sensorManager.KinectSensor = e.NewValue;
        }

        private void OnKinectSensorChooserPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void OnKinectSensorChanged(object sender, KinectSensorManagerChangedEventArgs<KinectSensor> e)
        {
            if (null != e.OldValue)
            {
                UninitializeKinectServices(e.KinectSensorManager, e.OldValue);
            }

            if ((null != e.NewValue) && (KinectStatus.Connected == e.NewValue.Status))
            {
                InitializeKinectServices(e.KinectSensorManager, e.NewValue);
            }
        }

        private void InitializeKinectServices(KinectSensorManager manager, KinectSensor sensor)
        {
            manager.ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;
            manager.ColorStreamEnabled = false;

            manager.DepthStreamEnabled = false;

            manager.SkeletonTransformSmoothParameters = new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            manager.SkeletonStreamEnabled = true;

            manager.KinectSensorEnabled = true;

            // skeleton
            sensor.AllFramesReady += OnKinectAllFramesReady;

            // audio
            sensor.AudioSource.AutomaticGainControlEnabled = false;
            sensor.AudioSource.Start();
            sensor.AudioSource.BeamAngleChanged += OnAudioSourceBeamAngleChanged;
            sensor.AudioSource.SoundSourceAngleChanged += OnAudioSourceSoundSourceAngleChanged;
        }

        private void UninitializeKinectServices(KinectSensorManager manager, KinectSensor sensor)
        {
            sensor.AudioSource.Stop();
            sensor.AudioSource.BeamAngleChanged -= OnAudioSourceBeamAngleChanged;
            sensor.AudioSource.SoundSourceAngleChanged -= OnAudioSourceSoundSourceAngleChanged;

            sensor.AllFramesReady -= OnKinectAllFramesReady;

            manager.ColorStreamEnabled = false;
            manager.DepthStreamEnabled = false;
            manager.SkeletonStreamEnabled = false;
            manager.KinectSensorEnabled = false;
        }
    }
}
