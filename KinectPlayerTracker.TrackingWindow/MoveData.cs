using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectPlayerTracker.TrackingWindow
{
    public class MoveData
    {
        public MoveDirection Direction { get; set; }
        public double Angle { get; set; }
        public double DepthMove { get; set; }
        public double Depth { get; set; }
        public double HorizontalMove { get; set; }
        public double Hypotenuse { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Direction[{0}], Angle[{1:###.####}], DepthMove[{2:###.###}], "
                + "Depth[{3:###.###}], HorizontalMove[{4:###.###}], Hypotenuse[{5:###.###}]",
                this.Direction,
                this.Angle,
                this.DepthMove,
                this.Depth,
                this.HorizontalMove,
                this.Hypotenuse); ;
        }
    }

    public enum MoveDirection
    {
        Stay = 0,
        Left = 1,
        Right = 2,
    }

    // 0x30 : stop     48
    // 0x31 : forward  49
    // 0x32 : left     50
    // 0x33 : backward 51
    // 0x34 : right    52
    public enum MoveCommand : byte
    {
        Stop = 0x30,
        Forward = 0x31,
        Left = 0x32,
        Backward = 0x33,
        Right = 0x34,
    }
}
