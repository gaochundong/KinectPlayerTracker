using System;
using Microsoft.Kinect;

namespace KinectPlayerTracker.TrackingWindow
{
    public partial class MainWindow
    {
        private double _currentSoundSourceAngle = 0;

        // 声源角度
        //     Gets the angle (in degrees) from kinect sensor towards the current sound
        //     source. When facing the Kinect: 0: center positive angles: right negative
        //     angles: left Range is -50 to +50 degrees.
        private void OnAudioSourceSoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            _currentSoundSourceAngle = e.Angle;
            Console.WriteLine(string.Format("{0}, {1}, {2}", "SoundSourceAngle", e.Angle, e.ConfidenceLevel));
        }

        // 波束方向变化
        //     Gets the angle (in degrees) of the direction towards which the audio beam
        //     is pointing, i.e.: the direction towards which the kinect sensor is listening.
        //     When facing the Kinect: 0: center positive angles: right negative angles:
        //     left Range is -50 to +50 degrees.
        private void OnAudioSourceBeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            Console.WriteLine(string.Format("{0}, {1}", "BeamAngle", e.Angle));
        }

        private void TurnRobot()
        {
            // 如果角度为正，说明用户在左侧
            if (_currentSoundSourceAngle > 0)
            {
                // 角度大于 40，用于在左侧
                if (_currentSoundSourceAngle >= 40)
                {
                    SendData(MoveDirection.Left, (int)(5000 * 90 / 360));
                }
                // 角度小于 30，用于在后方
                else if (_currentSoundSourceAngle <= 30)
                {
                    SendData(MoveDirection.Left, (int)(5000 * 180 / 360));
                }
            }
            // 如果角度为负，说明用户在右侧
            else if (_currentSoundSourceAngle < 0)
            {
                // 角度大于 40，用于在左侧
                if (-_currentSoundSourceAngle >= 40)
                {
                    SendData(MoveDirection.Right, (int)(5000 * 90 / 360));
                }
                // 角度小于 30，用于在后方
                else if (-_currentSoundSourceAngle <= 30)
                {
                    SendData(MoveDirection.Right, (int)(5000 * 180 / 360));
                }
            }
            else
            {
                SendData(MoveDirection.Stay, 0);
            }
        }
    }
}
