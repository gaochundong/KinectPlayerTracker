namespace KinectPlayerTracker.Toolkit
{
    public class KinectSensorManagerChangedEventArgs<T> : KinectSensorChangedEventArgs<T>
    {
        public KinectSensorManagerChangedEventArgs(KinectSensorManager sensorManager, T oldValue, T newValue)
          : base(oldValue, newValue)
        {
            this.KinectSensorManager = sensorManager;
        }

        public KinectSensorManager KinectSensorManager { get; private set; }
    }
}
