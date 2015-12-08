using System;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using KinectPlayerTracker.Sockets;

namespace KinectPlayerTracker.TrackingWindow
{
    public partial class MainWindow
    {
        private AsyncTcpClient _tcpClient;

        private void IntializeTcpConnection()
        {
            //IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999); // 测试用
            //IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("192.168.8.131"), 9998); // 测试用，可以不指定由系统选择端口
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("192.168.8.1"), 2001);
            _tcpClient = new AsyncTcpClient(remoteEP);
            _tcpClient.Encoding = Encoding.UTF8;
            _tcpClient.ServerExceptionOccurred += new EventHandler<TcpServerExceptionOccurredEventArgs>(OnTcpServerExceptionOccurred);
            _tcpClient.ServerConnected += new EventHandler<TcpServerConnectedEventArgs>(OnTcpServerConnected);
            _tcpClient.ServerDisconnected += new EventHandler<TcpServerDisconnectedEventArgs>(OnTcpServerDisconnected);
            _tcpClient.PlaintextReceived += new EventHandler<TcpDatagramReceivedEventArgs<string>>(OnTcpPlaintextReceived);

            Console.WriteLine("TCP client is connecting to server.");
            _tcpClient.Connect();
        }

        private void SendData(MoveDirection direction, int duration)
        {
            try
            {
                switch (direction)
                {
                    case MoveDirection.Left:
                        _tcpClient.Send((byte)MoveCommand.Right);
                        break;
                    case MoveDirection.Right:
                        _tcpClient.Send((byte)MoveCommand.Left);
                        break;
                    case MoveDirection.Stay:
                    default:
                        _tcpClient.Send((byte)MoveCommand.Stop);
                        break;
                }

                if (duration > 0)
                    Thread.Sleep(duration);
                _tcpClient.Send((byte)MoveCommand.Stop);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendData(MoveData data)
        {
            try
            {
                switch (data.Direction)
                {
                    case MoveDirection.Left:
                        _tcpClient.Send((byte)MoveCommand.Right);
                        break;
                    case MoveDirection.Right:
                        _tcpClient.Send((byte)MoveCommand.Left);
                        break;
                    case MoveDirection.Stay:
                    default:
                        _tcpClient.Send((byte)MoveCommand.Stop);
                        break;
                }

                Thread.Sleep((int)(5000 * data.Angle / 360));
                _tcpClient.Send((byte)MoveCommand.Stop);

                double moveable = data.Hypotenuse - 1;
                double moveDistance = Math.Abs(moveable);
                if (moveable > 0)
                {
                    // move forward
                    _tcpClient.Send((byte)MoveCommand.Forward);

                    Thread.Sleep((int)(5000 * moveDistance));
                    _tcpClient.Send((byte)MoveCommand.Stop);
                }
                else
                {
                    // move backward
                    _tcpClient.Send((byte)MoveCommand.Backward);

                    Thread.Sleep((int)(5000 * moveDistance));
                    _tcpClient.Send((byte)MoveCommand.Stop);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void OnTcpServerExceptionOccurred(object sender, TcpServerExceptionOccurredEventArgs e)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "TCP server {0} exception occurred, {1}.", e.ToString(), e.Exception.Message));
        }

        static void OnTcpServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "TCP server {0} has connected.", e.ToString()));
        }

        static void OnTcpServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "TCP server {0} has disconnected.", e.ToString()));
        }

        static void OnTcpPlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
        {
            Console.Write(string.Format("Server : {0} --> ", e.TcpClient.Client.RemoteEndPoint.ToString()));
            Console.WriteLine(string.Format("{0}", e.Datagram));
        }
    }
}
