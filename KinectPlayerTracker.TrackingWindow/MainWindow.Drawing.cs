using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using KinectPlayerTracker.Toolkit;
using Microsoft.Kinect;

namespace KinectPlayerTracker.TrackingWindow
{
    public partial class MainWindow
    {
        #region Skeleton Buffer

        private ImageType _imageType = ImageType.Color;
        private Skeleton[] _skeletonBuffer;

        private const int SkeletonCount = 6;
        private readonly List<Dictionary<JointType, JointMapping>> _jointMappings = new List<Dictionary<JointType, JointMapping>>()
      {
        new Dictionary<JointType, JointMapping>(),
        new Dictionary<JointType, JointMapping>(),
        new Dictionary<JointType, JointMapping>(),
        new Dictionary<JointType, JointMapping>(),
        new Dictionary<JointType, JointMapping>(),
        new Dictionary<JointType, JointMapping>(),
      };

        #endregion

        #region Draw Scale

        private double _scaleFactor = 1.0;
        private Size _renderSize = new Size(0, 0);

        #endregion

        #region Bone and Joint Brush

        private const double JointThickness = 3;
        private const double TrackedBoneThickness = 6;
        private const double InferredBoneThickness = 1;

        private readonly Pen _trackedBonePen = new Pen(Brushes.Green, TrackedBoneThickness);
        private readonly Pen _inferredBonePen = new Pen(Brushes.Gray, InferredBoneThickness);
        private readonly Brush _trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush _inferredJointBrush = Brushes.Yellow;

        #endregion

        #region Clipped Edges Brush

        private const double ClipBoundsThickness = 10;

        private readonly Brush _bottomClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 0), new Point(0, 1));
        private readonly Brush _topClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 1), new Point(0, 0));
        private readonly Brush _leftClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(1, 0), new Point(0, 0));
        private readonly Brush _rightClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 0), new Point(1, 0));

        #endregion

        #region Body Center Brush

        private const double BodyCenterThickness = 10;
        private readonly Brush _centerPointBrush = Brushes.Blue;

        #endregion

        private void OnKinectAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            KinectSensor sensor = sender as KinectSensor;

            if ((null == sensor) ||
                (null == sensor.SkeletonStream) ||
                !sensor.SkeletonStream.IsEnabled)
            {
                return;
            }

            bool haveSkeletonData = false;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((this._skeletonBuffer == null) || (this._skeletonBuffer.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this._skeletonBuffer = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this._skeletonBuffer);

                    haveSkeletonData = true;
                }
            }

            if (haveSkeletonData)
            {
                DecodeSkeletonData(e, sensor);
            }
        }

        private void DecodeSkeletonData(AllFramesReadyEventArgs e, KinectSensor sensor)
        {
            #region GetImageFormat

            ColorImageFormat colorFormat = ColorImageFormat.Undefined;
            int colorWidth = 0;
            int colorHeight = 0;

            DepthImageFormat depthFormat = DepthImageFormat.Undefined;
            int depthWidth = 0;
            int depthHeight = 0;

            switch (this._imageType)
            {
                case ImageType.Color:
                    // Retrieve the current color format, from the frame if present, and from the sensor if not.
                    using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
                    {
                        if (null != colorImageFrame)
                        {
                            colorFormat = colorImageFrame.Format;
                            colorWidth = colorImageFrame.Width;
                            colorHeight = colorImageFrame.Height;
                        }
                        else if (null != sensor.ColorStream)
                        {
                            colorFormat = sensor.ColorStream.Format;
                            colorWidth = sensor.ColorStream.FrameWidth;
                            colorHeight = sensor.ColorStream.FrameHeight;
                        }
                    }

                    break;
                case ImageType.Depth:
                    // Retrieve the current depth format, from the frame if present, and from the sensor if not.
                    using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
                    {
                        if (null != depthImageFrame)
                        {
                            depthFormat = depthImageFrame.Format;
                            depthWidth = depthImageFrame.Width;
                            depthHeight = depthImageFrame.Height;
                        }
                        else if (null != sensor.DepthStream)
                        {
                            depthFormat = sensor.DepthStream.Format;
                            depthWidth = sensor.DepthStream.FrameWidth;
                            depthHeight = sensor.DepthStream.FrameHeight;
                        }
                    }

                    break;
            }

            #endregion

            // Clear the play canvas
            this.playField.Children.Clear();

            // Check every skeleton
            for (int skeletonSlot = 0; skeletonSlot < this._skeletonBuffer.Length; skeletonSlot++)
            {
                var skeleton = this._skeletonBuffer[skeletonSlot];

                #region Skeleton Position

                // Map points between skeleton and color/depth
                var jointMapping = this._jointMappings[skeletonSlot];
                jointMapping.Clear();

                try
                {
                    // Transform the data into the correct space
                    // For each joint, we determine the exact X/Y coordinates for the target view
                    foreach (Joint joint in skeleton.Joints)
                    {
                        ColorImagePoint colorPoint = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, colorFormat);

                        Point mappedPoint = new Point(
                            (int)(this._renderSize.Width * colorPoint.X / colorWidth),
                            (int)(this._renderSize.Height * colorPoint.Y / colorHeight));

                        jointMapping[joint.JointType] = new JointMapping
                        {
                            Joint = joint,
                            MappedPoint = mappedPoint,
                            OriginPoint = colorPoint,
                        };
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Kinect is no longer available.
                    return;
                }

                // Look up the center point
                Point centerPoint = PositionCalculator.Get2DPosition(
                    sensor,
                    this._imageType,
                    this._renderSize,
                    skeleton.Position,
                    colorFormat,
                    colorWidth,
                    colorHeight,
                    depthFormat,
                    depthWidth,
                    depthHeight);

                #endregion

                // Scale the skeleton thickness
                // 1.0 is the desired size at 640 width
                this._scaleFactor = this._renderSize.Width / colorWidth;

                // Displays a gradient near the edge of the display 
                // where the skeleton is leaving the screen
                this.DrawClippedEdges(skeleton);

                switch (skeleton.TrackingState)
                {
                    case SkeletonTrackingState.PositionOnly:
                        {
                            // The skeleton is being tracked, but we only know the general position, and
                            // we do not know the specific joint locations.
                            this.DrawBodyCenter(centerPoint);
                        }
                        break;
                    case SkeletonTrackingState.Tracked:
                        {
                            // The skeleton is being tracked and the joint data is available for consumption.
                            this.DrawBody(skeleton, jointMapping);

                            // Track player
                            this.TrackPlayer(skeleton, jointMapping);
                        }
                        break;
                }
            }
        }

        private void DrawClippedEdges(Skeleton skeleton)
        {
            if (skeleton == null || skeleton.ClippedEdges.Equals(FrameEdges.None))
            {
                return;
            }

            double scaledThickness = ClipBoundsThickness * this._scaleFactor;

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                // Draw clipped edges rectangle
                Path path = new Path();
                path.Stroke = this._bottomClipBrush;
                path.StrokeThickness = scaledThickness;
                RectangleGeometry rectangle = new RectangleGeometry();
                rectangle.Rect = new Rect(0, this._renderSize.Height - scaledThickness, this._renderSize.Width, scaledThickness);
                path.Data = rectangle;

                // Add clipped edges into canvas
                this.playField.Children.Add(path);
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                // Draw clipped edges rectangle
                Path path = new Path();
                path.Stroke = this._topClipBrush;
                path.StrokeThickness = scaledThickness;
                RectangleGeometry rectangle = new RectangleGeometry();
                rectangle.Rect = new Rect(0, 0, this._renderSize.Width, scaledThickness);
                path.Data = rectangle;

                // Add clipped edges into canvas
                this.playField.Children.Add(path);
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                // Draw clipped edges rectangle
                Path path = new Path();
                path.Stroke = this._leftClipBrush;
                path.StrokeThickness = scaledThickness;
                RectangleGeometry rectangle = new RectangleGeometry();
                rectangle.Rect = new Rect(0, 0, scaledThickness, this._renderSize.Height);
                path.Data = rectangle;

                // Add clipped edges into canvas
                this.playField.Children.Add(path);
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                // Draw clipped edges rectangle
                Path path = new Path();
                path.Stroke = this._rightClipBrush;
                path.StrokeThickness = scaledThickness;
                RectangleGeometry rectangle = new RectangleGeometry();
                rectangle.Rect = new Rect(this._renderSize.Width - scaledThickness, 0, scaledThickness, this._renderSize.Height);
                path.Data = rectangle;

                // Add clipped edges into canvas
                this.playField.Children.Add(path);
            }
        }

        private void DrawBodyCenter(Point centerPoint)
        {
            Path path = new Path();
            path.Stroke = this._centerPointBrush;
            path.StrokeThickness = BodyCenterThickness;
            EllipseGeometry ellipse = new EllipseGeometry();
            ellipse.Center = centerPoint;
            ellipse.RadiusX = BodyCenterThickness * this._scaleFactor;
            ellipse.RadiusY = BodyCenterThickness * this._scaleFactor;
            path.Data = ellipse;

            this.playField.Children.Add(path);
        }

        private void DrawBody(Skeleton skeleton, Dictionary<JointType, JointMapping> jointMapping)
        {
            if (skeleton == null)
            {
                return;
            }

            // Render Torso
            this.DrawBone(jointMapping, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(jointMapping, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(jointMapping, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(jointMapping, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(jointMapping, JointType.Spine, JointType.HipCenter);
            this.DrawBone(jointMapping, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(jointMapping, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(jointMapping, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(jointMapping, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(jointMapping, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(jointMapping, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(jointMapping, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(jointMapping, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(jointMapping, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(jointMapping, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(jointMapping, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(jointMapping, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(jointMapping, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(jointMapping, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            this.DrawJoint(jointMapping);
        }

        private void DrawBone(Dictionary<JointType, JointMapping> jointMapping, JointType jointType1, JointType jointType2)
        {
            JointMapping joint1;
            JointMapping joint2;

            // If we can't find either of these joints, exit
            if (!jointMapping.TryGetValue(jointType1, out joint1) ||
                joint1.Joint.TrackingState == JointTrackingState.NotTracked ||
                !jointMapping.TryGetValue(jointType2, out joint2) ||
                joint2.Joint.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint1.Joint.TrackingState == JointTrackingState.Inferred &&
                joint2.Joint.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this._inferredBonePen;
            drawPen.Thickness = InferredBoneThickness * this._scaleFactor;
            if (joint1.Joint.TrackingState == JointTrackingState.Tracked
              && joint2.Joint.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this._trackedBonePen;
                drawPen.Thickness = TrackedBoneThickness * this._scaleFactor;
            }

            // Draw bone line
            Path bone = new Path();
            bone.Stroke = drawPen.Brush;
            bone.StrokeThickness = drawPen.Thickness;
            LineGeometry boneLine = new LineGeometry();
            boneLine.StartPoint = joint1.MappedPoint;
            boneLine.EndPoint = joint2.MappedPoint;
            bone.Data = boneLine;

            // Add bone into canvas
            this.playField.Children.Add(bone);
        }

        private void DrawJoint(Dictionary<JointType, JointMapping> jointMapping)
        {
            foreach (JointMapping joint in jointMapping.Values)
            {
                Brush drawBrush = null;
                switch (joint.Joint.TrackingState)
                {
                    case JointTrackingState.Tracked:
                        drawBrush = this._trackedJointBrush;
                        break;
                    case JointTrackingState.Inferred:
                        drawBrush = this._inferredJointBrush;
                        break;
                }

                if (drawBrush != null)
                {
                    // Draw joint ellipse
                    Path jointPath = new Path();
                    jointPath.Stroke = drawBrush;
                    jointPath.StrokeThickness = JointThickness * this._scaleFactor;
                    EllipseGeometry jointEllipse = new EllipseGeometry();
                    jointEllipse.Center = joint.MappedPoint;
                    jointEllipse.RadiusX = JointThickness * this._scaleFactor;
                    jointEllipse.RadiusY = JointThickness * this._scaleFactor;
                    jointPath.Data = jointEllipse;

                    // Add joint into canvas
                    this.playField.Children.Add(jointPath);
                }
            }
        }
    }

    /// <summary>
    /// This class is used to map points between skeleton and color/depth
    /// </summary>
    public class JointMapping
    {
        /// <summary>
        /// Gets or sets the joint at which we we are looking
        /// </summary>
        public Joint Joint { get; set; }

        /// <summary>
        /// Gets or sets the point mapped into the target display
        /// </summary>
        public Point MappedPoint { get; set; }

        public ColorImagePoint OriginPoint { get; set; }

        public override string ToString()
        {
            return string.Format("JointType[{0}], TrackingState[{1}], Coordinate[{2}], OriginPoint[{3}], MappedPoint[{4}]",
              Joint.JointType,
              Joint.TrackingState,
              string.Format("{0},{1},{2}", Joint.Position.X, Joint.Position.Y, Joint.Position.Z),
              string.Format("{0},{1}", OriginPoint.X, OriginPoint.Y),
              string.Format("{0},{1}", MappedPoint.X, MappedPoint.Y));
        }
    }
}
