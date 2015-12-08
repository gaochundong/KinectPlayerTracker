using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace KinectPlayerTracker.Toolkit
{
    public static class PositionCalculator
    {
        /// <summary>
        /// Returns the 2D position of the provided 3D SkeletonPoint.
        /// The result will be in in either Color coordinate space or Depth coordinate space, depending on 
        /// the current value of this.ImageType.
        /// Only those parameters associated with the current ImageType will be used.
        /// </summary>
        /// <param name="sensor">The KinectSensor for which this mapping is being performed.</param>
        /// <param name="imageType">The target image type</param>
        /// <param name="renderSize">The target dimensions of the visualization</param>
        /// <param name="skeletonPoint">The source point to map</param>
        /// <param name="colorFormat">The format of the target color image, if imageType is Color</param>
        /// <param name="colorWidth">The width of the target color image, if the imageType is Color</param>
        /// <param name="colorHeight">The height of the target color image, if the imageType is Color</param>
        /// <param name="depthFormat">The format of the target depth image, if the imageType is Depth</param>
        /// <param name="depthWidth">The width of the target depth image, if the imageType is Depth</param>
        /// <param name="depthHeight">The height of the target depth image, if the imageType is Depth</param>
        /// <returns>Returns the 2D position of the provided 3D SkeletonPoint.</returns>
        public static Point Get2DPosition(
            KinectSensor sensor,
            ImageType imageType,
            Size renderSize,
            SkeletonPoint skeletonPoint,
            ColorImageFormat colorFormat,
            int colorWidth,
            int colorHeight,
            DepthImageFormat depthFormat,
            int depthWidth,
            int depthHeight)
        {
            try
            {
                switch (imageType)
                {
                    case ImageType.Color:
                        if (ColorImageFormat.Undefined != colorFormat)
                        {
                            var colorPoint = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, colorFormat);

                            return new Point(
                                (int)(renderSize.Width * colorPoint.X / colorWidth),
                                (int)(renderSize.Height * colorPoint.Y / colorHeight));
                        }

                        break;
                    case ImageType.Depth:
                        if (DepthImageFormat.Undefined != depthFormat)
                        {
                            var depthPoint = sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, depthFormat);

                            return new Point(
                                (int)(renderSize.Width * depthPoint.X / depthWidth),
                                (int)(renderSize.Height * depthPoint.Y / depthHeight));
                        }

                        break;
                }
            }
            catch (InvalidOperationException)
            {
                // The stream must have stopped abruptly
                // Handle this gracefully
            }

            return new Point();
        }
    }
}
