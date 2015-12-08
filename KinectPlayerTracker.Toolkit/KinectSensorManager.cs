using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;

namespace KinectPlayerTracker.Toolkit
{
    public class KinectSensorManager : Freezable
    {
        #region DependencyProperty

        public static readonly DependencyProperty KinectSensorProperty;

        public KinectSensor KinectSensor
        {
            get { return (KinectSensor)this.GetValue(KinectSensorProperty); }
            set { this.SetValue(KinectSensorProperty, value); }
        }

        public static readonly DependencyProperty KinectSensorStatusProperty;

        public KinectStatus KinectSensorStatus
        {
            get { return (KinectStatus)this.GetValue(KinectSensorStatusProperty); }
            set { this.SetValue(KinectSensorStatusProperty, value); }
        }

        public static readonly DependencyProperty KinectSensorEnabledProperty;

        public bool KinectSensorEnabled
        {
            get { return (bool)this.GetValue(KinectSensorEnabledProperty); }
            set { this.SetValue(KinectSensorEnabledProperty, value); }
        }

        public static readonly DependencyProperty KinectSensorAppConflictProperty;

        public bool KinectSensorAppConflict
        {
            get { return (bool)this.GetValue(KinectSensorAppConflictProperty); }
            private set { this.SetValue(KinectSensorAppConflictProperty, value); }
        }

        public static readonly DependencyProperty UniqueKinectIdProperty;

        public string UniqueKinectId
        {
            get { return (string)this.GetValue(UniqueKinectIdProperty); }
            private set { this.SetValue(UniqueKinectIdProperty, value); }
        }

        public static readonly DependencyProperty SupportsCameraSettingsProperty;

        public bool SupportsCameraSettings
        {
            get { return (bool)this.GetValue(SupportsCameraSettingsProperty); }
            set { this.SetValue(SupportsCameraSettingsProperty, value); }
        }

        public static readonly DependencyProperty CameraSettingsProperty;

        public ColorCameraSettings CameraSettings
        {
            get { return (ColorCameraSettings)this.GetValue(CameraSettingsProperty); }
            set { this.SetValue(CameraSettingsProperty, value); }
        }

        public static readonly DependencyProperty ColorStreamEnabledProperty;

        public bool ColorStreamEnabled
        {
            get { return (bool)this.GetValue(KinectSensorManager.ColorStreamEnabledProperty); }
            set { this.SetValue(KinectSensorManager.ColorStreamEnabledProperty, value); }
        }

        public static readonly DependencyProperty ColorFormatProperty;

        public ColorImageFormat ColorFormat
        {
            get { return (ColorImageFormat)this.GetValue(KinectSensorManager.ColorFormatProperty); }
            set { this.SetValue(KinectSensorManager.ColorFormatProperty, value); }
        }

        public static readonly DependencyProperty DepthStreamEnabledProperty;

        public bool DepthStreamEnabled
        {
            get { return (bool)this.GetValue(KinectSensorManager.DepthStreamEnabledProperty); }
            set { this.SetValue(KinectSensorManager.DepthStreamEnabledProperty, value); }
        }

        public static readonly DependencyProperty DepthFormatProperty;

        public DepthImageFormat DepthFormat
        {
            get { return (DepthImageFormat)this.GetValue(KinectSensorManager.DepthFormatProperty); }
            set { this.SetValue(KinectSensorManager.DepthFormatProperty, value); }
        }

        public static readonly DependencyProperty DepthRangeProperty;

        public DepthRange DepthRange
        {
            get { return (DepthRange)this.GetValue(KinectSensorManager.DepthRangeProperty); }
            set { this.SetValue(KinectSensorManager.DepthRangeProperty, value); }
        }

        public static readonly DependencyProperty SkeletonStreamEnabledProperty;

        public bool SkeletonStreamEnabled
        {
            get { return (bool)this.GetValue(KinectSensorManager.SkeletonStreamEnabledProperty); }
            set { this.SetValue(KinectSensorManager.SkeletonStreamEnabledProperty, value); }
        }

        public static readonly DependencyProperty SkeletonTrackingModeProperty;

        public SkeletonTrackingMode SkeletonTrackingMode
        {
            get { return (SkeletonTrackingMode)this.GetValue(KinectSensorManager.SkeletonTrackingModeProperty); }
            set { this.SetValue(KinectSensorManager.SkeletonTrackingModeProperty, value); }
        }

        public static readonly DependencyProperty SkeletonEnableTrackingInNearModeProperty;

        public bool SkeletonEnableTrackingInNearMode
        {
            get { return (bool)GetValue(SkeletonEnableTrackingInNearModeProperty); }
            set { SetValue(SkeletonEnableTrackingInNearModeProperty, value); }
        }

        public static readonly DependencyProperty SkeletonTransformSmoothParametersProperty;

        public TransformSmoothParameters SkeletonTransformSmoothParameters
        {
            get { return (TransformSmoothParameters)this.GetValue(SkeletonTransformSmoothParametersProperty); }
            set { this.SetValue(SkeletonTransformSmoothParametersProperty, value); }
        }

        public static readonly DependencyProperty ElevationAngleProperty;

        public int ElevationAngle
        {
            get { return (int)this.GetValue(KinectSensorManager.ElevationAngleProperty); }
            set { this.SetValue(KinectSensorManager.ElevationAngleProperty, value); }
        }

        public static readonly DependencyProperty ForceInfraredEmitterOffProperty;

        public bool ForceInfraredEmitterOff
        {
            get { return (bool)GetValue(ForceInfraredEmitterOffProperty); }
            set { SetValue(ForceInfraredEmitterOffProperty, value); }
        }

        public static readonly DependencyProperty EnableAccelerometerReadingProperty;

        public bool EnableAccelerometerReading
        {
            get { return (bool)GetValue(EnableAccelerometerReadingProperty); }
            set { SetValue(EnableAccelerometerReadingProperty, value); }
        }

        public static readonly DependencyProperty AccelerometerProperty;

        public Vector4 Accelerometer
        {
            get { return (Vector4)this.GetValue(KinectSensorManager.AccelerometerProperty); }
            set { this.SetValue(KinectSensorManager.AccelerometerProperty, value); }
        }

        static KinectSensorManager()
        {
            KinectSensorProperty =
                DependencyProperty.Register(
                    "KinectSensor",
                    typeof(KinectSensor),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(null, OnKinectSensorOrStatusChanged));

            KinectSensorStatusProperty =
                DependencyProperty.Register(
                    "KinectSensorStatus",
                    typeof(KinectStatus),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(KinectStatus.Undefined, OnKinectSensorOrStatusChanged));

            KinectSensorEnabledProperty =
                DependencyProperty.Register(
                    "KinectSensorEnabled",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(true, (sender, args) => ((KinectSensorManager)sender).EnsureKinectSensorRunningState()));

            KinectSensorAppConflictProperty =
                DependencyProperty.Register(
                    "KinectSensorAppConflict",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(false, (sender, args) => ((KinectSensorManager)sender).NotifyAppConflict()));

            UniqueKinectIdProperty =
                DependencyProperty.Register(
                    "UniqueKinectId",
                    typeof(string),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(null));

            SupportsCameraSettingsProperty =
                DependencyProperty.Register(
                    "SupportsCameraSettings",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(false));

            CameraSettingsProperty =
                DependencyProperty.Register(
                    "CameraSettings",
                    typeof(ColorCameraSettings),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(null));

            ColorStreamEnabledProperty =
                DependencyProperty.Register(
                    "ColorStreamEnabled",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(true, (sender, args) => ((KinectSensorManager)sender).EnsureColorStreamState()));

            ColorFormatProperty =
                DependencyProperty.Register(
                    "ColorFormat",
                    typeof(ColorImageFormat),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(
                        ColorImageFormat.RgbResolution640x480Fps30,
                        (sender, args) => ((KinectSensorManager)sender).EnsureColorStreamState()));

            DepthStreamEnabledProperty =
                DependencyProperty.Register(
                    "DepthStreamEnabled",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(true, (sender, args) => ((KinectSensorManager)sender).EnsureDepthStreamState()));

            DepthFormatProperty =
                DependencyProperty.Register(
                    "DepthFormat",
                    typeof(DepthImageFormat),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(DepthImageFormat.Resolution640x480Fps30, (sender, args) => ((KinectSensorManager)sender).EnsureDepthStreamState()));

            DepthRangeProperty =
                DependencyProperty.Register(
                    "DepthRange",
                    typeof(DepthRange),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(DepthRange.Default, (sender, args) => ((KinectSensorManager)sender).EnsureDepthStreamState()));

            SkeletonStreamEnabledProperty =
                DependencyProperty.Register(
                    "SkeletonStreamEnabled",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(
                        false,
                        (sender, args) => ((KinectSensorManager)sender).EnsureSkeletonStreamState()));

            SkeletonTrackingModeProperty =
                DependencyProperty.Register(
                    "SkeletonTrackingMode",
                    typeof(SkeletonTrackingMode),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(
                        SkeletonTrackingMode.Default,
                        (sender, args) => ((KinectSensorManager)sender).EnsureSkeletonStreamState()));

            SkeletonEnableTrackingInNearModeProperty =
                DependencyProperty.Register(
                    "SkeletonEnableTrackingInNearMode",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(
                        true,
                        (sender, args) => ((KinectSensorManager)sender).EnsureSkeletonStreamState()));

            SkeletonTransformSmoothParametersProperty =
                DependencyProperty.Register(
                    "SkeletonTransformSmoothParameters",
                    typeof(TransformSmoothParameters),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(
                        default(TransformSmoothParameters),
                        (sender, args) => ((KinectSensorManager)sender).EnsureSkeletonStreamState()));

            ElevationAngleProperty =
                DependencyProperty.Register(
                    "ElevationAngle",
                    typeof(int),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(0, (sender, args) => ((KinectSensorManager)sender).EnsureElevationAngle(), CoerceElevationAngle));

            ForceInfraredEmitterOffProperty =
                DependencyProperty.Register(
                    "ForceInfraredEmitterOff",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(false, (sender, args) => ((KinectSensorManager)sender).EnsureForceInfraredEmitterOff()));

            EnableAccelerometerReadingProperty =
                DependencyProperty.Register(
                    "EnableAccelerometerReading",
                    typeof(bool),
                    typeof(KinectSensorManager),
                    new PropertyMetadata(false));

            AccelerometerProperty =
                DependencyProperty.Register(
                    "Accelerometer",
                    typeof(Vector4),
                    typeof(KinectSensorManager));
        }

        #endregion

        #region Sensor Changed

        public event EventHandler<KinectSensorManagerChangedEventArgs<KinectSensor>> KinectSensorChanged;

        public event EventHandler<KinectSensorManagerChangedEventArgs<KinectStatus>> KinectStatusChanged;

        public event EventHandler<KinectSensorManagerChangedEventArgs<bool>> KinectRunningStateChanged;

        private static void OnKinectSensorOrStatusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var sensorManager = sender as KinectSensorManager;

            if (null == sensorManager)
            {
                return;
            }

            var oldSensor = sensorManager.KinectSensor;
            var newSensor = sensorManager.KinectSensor;
            var oldStatus = KinectStatus.Undefined;
            var newStatus = null == newSensor ? KinectStatus.Undefined : newSensor.Status;

            bool sensorChanged = KinectSensorManager.KinectSensorProperty == args.Property;
            bool statusChanged = KinectSensorManager.KinectSensorStatusProperty == args.Property;

            if (sensorChanged)
            {
                oldSensor = (KinectSensor)args.OldValue;

                // The elevation task is per-sensor
                sensorManager._isElevationTaskOutstanding = false;

                // This can throw if the sensor is going away or gone.
                try
                {
                    sensorManager.UniqueKinectId = (null == newSensor) ? null : newSensor.UniqueKinectId;
                }
                catch (InvalidOperationException) { }
            }

            if (statusChanged)
            {
                oldStatus = (KinectStatus)args.OldValue;
            }

            // Ensure that the sensor is uninitialized if the sensor has changed or if the status is not Connected
            if (sensorChanged || (statusChanged && (KinectStatus.Connected != newStatus)))
            {
                sensorManager.UninitializeKinectServices(oldSensor);
            }

            bool wasRunning = (null != newSensor) && newSensor.IsRunning;

            sensorManager.InitializeKinectServices();

            bool isRunning = (null != newSensor) && newSensor.IsRunning;

            sensorManager.KinectSensorStatus = newStatus;

            if (sensorChanged && (null != sensorManager.KinectSensorChanged))
            {
                sensorManager.KinectSensorChanged(sensorManager, new KinectSensorManagerChangedEventArgs<KinectSensor>(sensorManager, oldSensor, newSensor));
            }

            if ((newStatus != oldStatus) && (null != sensorManager.KinectStatusChanged))
            {
                sensorManager.KinectStatusChanged(sensorManager, new KinectSensorManagerChangedEventArgs<KinectStatus>(sensorManager, oldStatus, newStatus));
            }

            if ((wasRunning != isRunning) && (null != sensorManager.KinectRunningStateChanged))
            {
                sensorManager.KinectRunningStateChanged(sensorManager, new KinectSensorManagerChangedEventArgs<bool>(sensorManager, wasRunning, isRunning));
            }

            try
            {
                if (newSensor != null && newSensor.ColorStream.CameraSettings != null)
                {
                    sensorManager.SupportsCameraSettings = true;
                    sensorManager.CameraSettings = newSensor.ColorStream.CameraSettings;
                }
                else
                {
                    sensorManager.SupportsCameraSettings = false;
                    sensorManager.CameraSettings = null;
                }
            }
            catch (InvalidOperationException)
            {
                sensorManager.SupportsCameraSettings = false;
                sensorManager.CameraSettings = null;
            }
        }

        #endregion

        #region Sensor Conflict

        private void NotifyAppConflict()
        {
            if (null != this.KinectAppConflictChanged)
            {
                this.KinectAppConflictChanged(this, new KinectSensorManagerChangedEventArgs<bool>(this, !this.KinectSensorAppConflict, this.KinectSensorAppConflict));
            }
        }

        public event EventHandler<KinectSensorManagerChangedEventArgs<bool>> KinectAppConflictChanged;

        #endregion

        #region Sensor Initialization

        private void InitializeKinectServices()
        {
            this.EnsureColorStreamState();
            this.EnsureDepthStreamState();
            this.EnsureSkeletonStreamState();
            this.EnsureElevationAngle();
            this.EnsureForceInfraredEmitterOff();
            this.EnsureKinectSensorRunningState();
            this.EnsureAccelerometer();
        }

        private void EnsureColorStreamState()
        {
            var sensor = this.KinectSensor;

            if ((null == sensor) || (KinectStatus.Connected != sensor.Status))
            {
                return;
            }

            if (this.ColorStreamEnabled)
            {
                try
                {
                    sensor.ColorStream.Enable(this.ColorFormat);
                }
                catch (InvalidOperationException)
                {
                    // The device went away while we were trying to start
                    sensor.ColorStream.Disable();
                    return;
                }
            }
            else
            {
                sensor.ColorStream.Disable();
            }
        }

        private void EnsureDepthStreamState()
        {
            var sensor = this.KinectSensor;

            if ((null == sensor) || (KinectStatus.Connected != sensor.Status))
            {
                return;
            }

            if (this.DepthStreamEnabled)
            {
                try
                {
                    sensor.DepthStream.Enable(this.DepthFormat);
                }
                catch (InvalidOperationException)
                {
                    // The device went away while we were trying to start
                    sensor.DepthStream.Disable();
                    return;
                }

                // If this call causes an InvalidOperationException, this means that the device
                // does not support NearMode.
                try
                {
                    sensor.DepthStream.Range = this.DepthRange;
                }
                catch (InvalidOperationException)
                {
                    this.DepthRange = DepthRange.Default;
                }
            }
            else
            {
                sensor.DepthStream.Disable();
            }
        }

        private void EnsureSkeletonStreamState()
        {
            var sensor = this.KinectSensor;

            if ((null == sensor) || (KinectStatus.Connected != sensor.Status))
            {
                return;
            }

            bool skeletonChangeWillCauseAudioReset = sensor.SkeletonStream.IsEnabled != this.SkeletonStreamEnabled;

            if (this.SkeletonStreamEnabled)
            {
                try
                {
                    sensor.SkeletonStream.Enable(this.SkeletonTransformSmoothParameters);
                    sensor.SkeletonStream.TrackingMode = this.SkeletonTrackingMode;
                    sensor.SkeletonStream.EnableTrackingInNearRange = this.SkeletonEnableTrackingInNearMode;
                }
                catch (InvalidOperationException)
                {
                    // The device went away while we were trying to start
                    sensor.SkeletonStream.Disable();
                    return;
                }
            }
            else
            {
                sensor.SkeletonStream.Disable();
            }
        }

        private void EnsureElevationAngle()
        {
            var sensor = this.KinectSensor;

            // We cannot set the angle on a sensor if it is not running.
            // We will therefore call EnsureElevationAngle when the requested angle has changed or if the
            // sensor transitions to the Running state.
            if ((null == sensor) || (KinectStatus.Connected != sensor.Status) || !sensor.IsRunning)
            {
                return;
            }

            this._targetElevationAngle = this.ElevationAngle;

            // If there already a background task, it will notice the new targetElevationAngle
            if (!this._isElevationTaskOutstanding)
            {
                // Otherwise, we need to start a new task
                this.StartElevationTask();
            }
        }

        private void EnsureForceInfraredEmitterOff()
        {
            var sensor = this.KinectSensor;

            if ((null == sensor) || (KinectStatus.Connected != sensor.Status))
            {
                return;
            }

            // If this call causes an InvalidOperationException, this means that the device
            // does not support IR emitter changes.
            try
            {
                sensor.ForceInfraredEmitterOff = this.ForceInfraredEmitterOff;
            }
            catch (InvalidOperationException)
            {
                this.ForceInfraredEmitterOff = false;
            }
        }

        private void EnsureKinectSensorRunningState()
        {
            var sensor = this.KinectSensor;

            if ((null == sensor) || (KinectStatus.Connected != sensor.Status))
            {
                return;
            }

            if (this.KinectSensorEnabled)
            {
                if (!sensor.IsRunning)
                {
                    // If this call causes an IOException, this means that the sensor is in use by
                    // another application.
                    try
                    {
                        sensor.Start();

                        // We need to retrieve the elevation angle here because the angle may only be retrieved
                        // on a running sensor.
                        this.ElevationAngle = sensor.ElevationAngle;
                        this.KinectSensorAppConflict = false;
                    }
                    catch (IOException)
                    {
                        this.KinectSensorAppConflict = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // The device went away while we were trying to start
                        sensor.Stop();
                        this.KinectSensorAppConflict = false;
                    }
                }
            }
            else
            {
                sensor.Stop();
                this.KinectSensorAppConflict = false;
            }
        }

        private void EnsureAccelerometer()
        {
            var sensor = this.KinectSensor;

            // We cannot set the angle on a sensor if it is not running.
            // We will therefore call EnsureElevationAngle when the requested angle has changed or if the
            // sensor transitions to the Running state.
            if ((null == sensor) || (KinectStatus.Connected != sensor.Status))
            {
                return;
            }

            if (EnableAccelerometerReading)
            {
                sensor.AllFramesReady += AccelerometerAllFramesReady;
            }
            else
            {
                sensor.AllFramesReady -= AccelerometerAllFramesReady;
            }
        }

        private void AccelerometerAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Have we already been "shut down"
            if (KinectSensor == null)
            {
                return;
            }

            if (EnableAccelerometerReading)
            {
                this.Accelerometer = KinectSensor.AccelerometerGetCurrentReading();
            }
        }

        #endregion

        #region Sensor Uninitialization

        private void UninitializeKinectServices(KinectSensor sensor)
        {
            if (null == sensor)
            {
                return;
            }

            // Stop streaming
            sensor.Stop();

            sensor.AllFramesReady -= AccelerometerAllFramesReady;

            this.KinectSensorAppConflict = false;

            if (null != sensor.AudioSource)
            {
                sensor.AudioSource.Stop();
            }

            if (null != sensor.SkeletonStream)
            {
                sensor.SkeletonStream.Disable();
            }

            if (null != sensor.DepthStream)
            {
                sensor.DepthStream.Disable();
            }

            if (null != sensor.ColorStream)
            {
                sensor.ColorStream.Disable();
            }
        }

        #endregion

        #region Sensor Elevation Angle

        private int _targetElevationAngle = int.MinValue;
        private bool _isElevationTaskOutstanding;

        private void StartElevationTask()
        {
            var sensor = this.KinectSensor;
            int lastSetElevationAngle = int.MinValue;

            if (null != sensor)
            {
                this._isElevationTaskOutstanding = true;

                Task.Factory
                  .StartNew(() =>
                  {
                      int angleToSet = this._targetElevationAngle;

              // Keep going until we "match", assuming that the sensor is running
              while ((lastSetElevationAngle != angleToSet) && sensor.IsRunning)
                      {
                  // We must wait at least 1 second, and call no more frequently than 15 times every 20 seconds
                  // So, we wait at least 1350ms afterwards before we set backgroundUpdateInProgress to false.
                  sensor.ElevationAngle = angleToSet;
                          lastSetElevationAngle = angleToSet;
                          Thread.Sleep(1350);

                          angleToSet = this._targetElevationAngle;
                      }
                  })
                  .ContinueWith(results =>
                  {
              // This can happen if the Kinect transitions from Running to not running
              // after the check above but before setting the ElevationAngle.
              if (results.IsFaulted)
                      {
                          var exception = results.Exception;

                          Debug.WriteLine(
                      "Set Elevation Task failed with exception " +
                      exception);
                      }

              // We caught up and handled all outstanding angle requests.
              // However, more may come in after we've stopped checking, so
              // we post this work item back to the main thread to determine
              // whether we need to start the task up again.
              this.Dispatcher.BeginInvoke((Action)(() =>
              {
                        if (this._targetElevationAngle !=
                    lastSetElevationAngle)
                        {
                            this.StartElevationTask();
                        }
                        else
                        {
                    // If there's nothing to do, we can set this to false.
                    this._isElevationTaskOutstanding = false;
                        }
                    }));
                  });
            }
        }

        private static object CoerceElevationAngle(DependencyObject sender, object baseValue)
        {
            var sensorWrapper = sender as KinectSensorManager;

            if ((null == sensorWrapper) || !(baseValue is int))
            {
                return 0;
            }

            // Best guess default values for min/max angles
            int minVal = -27;
            int maxVal = 27;

            if (null != sensorWrapper.KinectSensor)
            {
                minVal = sensorWrapper.KinectSensor.MinElevationAngle;
                maxVal = sensorWrapper.KinectSensor.MaxElevationAngle;
            }

            if ((int)baseValue < minVal)
            {
                return minVal;
            }

            if ((int)baseValue > maxVal)
            {
                return maxVal;
            }

            return baseValue;
        }

        #endregion

        #region Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new KinectSensorManager();
        }

        protected override bool FreezeCore(bool isChecking)
        {
            return (null == this.KinectSensor) && base.FreezeCore(isChecking);
        }

        #endregion
    }
}