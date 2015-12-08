using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectPlayerTracker.Toolkit
{
    public class KinectSensorChangedEventArgs<T> : EventArgs
    {
        public KinectSensorChangedEventArgs(T oldValue, T newValue)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }

        public T OldValue { get; private set; }

        public T NewValue { get; private set; }
    }
}
