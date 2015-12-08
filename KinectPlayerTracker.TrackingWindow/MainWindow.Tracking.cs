using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using KinectPlayerTracker.Sockets;
using KinectPlayerTracker.Toolkit;
using Microsoft.Kinect;

namespace KinectPlayerTracker.TrackingWindow
{
    public partial class MainWindow
    {
        private int _primaryTrackingPlayerId = -1;
        private DateTime _playerLastTrackedTime = DateTime.Now;
        private Dictionary<JointType, JointMapping> _playerLastJointMapping;
        private object _lockOfOnlyTrackOnePlayer = new object();
        private int _rightHandShakingGestureFlag = 0;
        private double _minRightHandShakeThreshold = 0.2;

        private void TrackPlayer(Skeleton skeleton, Dictionary<JointType, JointMapping> jointMapping)
        {
            if (Monitor.TryEnter(_lockOfOnlyTrackOnePlayer))
            {
                try
                {
                    // find who is shaking hand
                    if (_primaryTrackingPlayerId == -1)
                    {
                        if (IsShakingRightHand(skeleton))
                        {
                            _rightHandShakingGestureFlag = 0;
                            _primaryTrackingPlayerId = skeleton.TrackingId;
                        }
                        else
                        {
                            // cannot find gesture, then try audio
                            if (_rightHandShakingGestureFlag > 90)
                            {
                                _rightHandShakingGestureFlag = 0;
                                TurnRobot();
                            }
                            return;
                        }
                    }

                    // only track one player at a time
                    if (skeleton.TrackingId != _primaryTrackingPlayerId)
                    {
                        // -1 is no traking player, need find player again
                        _primaryTrackingPlayerId = -1;
                        _playerLastJointMapping = null;
                        _playerLastTrackedTime = DateTime.Now;
                        return;
                    }

                    // move robot
                    TurnCenter(jointMapping);
                    _playerLastJointMapping = new Dictionary<JointType, JointMapping>(jointMapping);
                    _playerLastTrackedTime = DateTime.Now;
                }
                finally
                {
                    Monitor.Exit(_lockOfOnlyTrackOnePlayer);
                }
            }
        }

        private bool IsShakingRightHand(Skeleton s)
        {
            Joint rightHandJoint = s.Joints
              .Where(j => j.JointType == JointType.HandRight)
              .FirstOrDefault();
            Joint shoulderRightJoint = s.Joints
              .Where(j => j.JointType == JointType.ShoulderRight)
              .FirstOrDefault();

            if (!(rightHandJoint.TrackingState == JointTrackingState.Tracked
              && shoulderRightJoint.TrackingState == JointTrackingState.Tracked))
                return false;

            // 右手高于右肩
            if (rightHandJoint.Position.Y > shoulderRightJoint.Position.Y)
            {
                // 判断振臂操作，记录两次右手Z和右肩Z的差值，阈值是经验值可调整
                if (Math.Abs(shoulderRightJoint.Position.Z - rightHandJoint.Position.Z) < _minRightHandShakeThreshold)
                    _rightHandShakingGestureFlag++;
                else if (_rightHandShakingGestureFlag > 0 && Math.Abs(shoulderRightJoint.Position.Z - rightHandJoint.Position.Z) > _minRightHandShakeThreshold)
                {
                    _rightHandShakingGestureFlag = 0;
                    return true;
                }
            }

            return false;
        }

        private void TurnCenter(Dictionary<JointType, JointMapping> playerCurrentJointMapping)
        {
            double currentDistance = playerCurrentJointMapping[JointType.HipCenter].Joint.Position.Z;
            var currentPostion = playerCurrentJointMapping[JointType.HipCenter].OriginPoint;

            double xMove = currentPostion.X - 320;
            double yMove = currentPostion.Y - 240;
            double zMove = currentDistance - 1;

            bool isMovedToLeft = xMove < 0;
            bool isMovedToRight = xMove > 0;

            double x = Math.Abs(xMove);
            double y = Math.Abs(yMove);
            double z = Math.Abs(zMove);

            double w = Math.Tan(28.5 * (Math.PI / 180)) * currentDistance;
            double a = x * (w / 320);
            double tanA = a / currentDistance;
            double radianA = Math.Atan(tanA);
            double angleA = radianA / (Math.PI / 180);
            double c = Math.Sqrt(a * a + currentDistance * currentDistance);

            if (x > 0)
            {
                var moveData = new MoveData()
                {
                    Direction = isMovedToLeft ? MoveDirection.Left : isMovedToRight ? MoveDirection.Right : MoveDirection.Stay,
                    Angle = angleA,
                    DepthMove = z,
                    Depth = currentDistance,
                    HorizontalMove = a,
                    Hypotenuse = c,
                };

                this.textField.Text = moveData.ToString();

                // send data to robot
                SendData(moveData);
            }
        }

        private void MoveRobot(Dictionary<JointType, JointMapping> playerCurrentJointMapping, Dictionary<JointType, JointMapping> playerLastJointMapping)
        {
            double currentDistance = playerCurrentJointMapping[JointType.HipCenter].Joint.Position.Z;
            double lastDistance = playerLastJointMapping[JointType.HipCenter].Joint.Position.Z;
            var currentPostion = playerCurrentJointMapping[JointType.HipCenter].OriginPoint;
            var lastPostion = playerLastJointMapping[JointType.HipCenter].OriginPoint;

            double xMove = currentPostion.X - lastPostion.X;
            double yMove = currentPostion.Y - lastPostion.Y;
            double zMove = currentDistance - lastDistance;

            bool isMovedToLeft = xMove < 0;
            bool isMovedToRight = xMove > 0;

            double x = Math.Abs(xMove);
            double y = Math.Abs(yMove);
            double z = Math.Abs(zMove);

            double w = Math.Tan(28.5 * (Math.PI / 180)) * currentDistance;
            double a = x * (w / 320);
            double tanA = a / currentDistance;
            double radianA = Math.Atan(tanA);
            double angleA = radianA / (Math.PI / 180);
            double c = Math.Sqrt(a * a + currentDistance * currentDistance);

            if (x > 0)
            {
                var moveData = new MoveData()
                {
                    Direction = isMovedToLeft ? MoveDirection.Left : isMovedToRight ? MoveDirection.Right : MoveDirection.Stay,
                    Angle = angleA,
                    DepthMove = z,
                    Depth = currentDistance,
                    HorizontalMove = a,
                    Hypotenuse = c,
                };

                this.textField.Text = moveData.ToString();

                // send data to robot
                SendData(moveData);
            }
        }
    }
}
