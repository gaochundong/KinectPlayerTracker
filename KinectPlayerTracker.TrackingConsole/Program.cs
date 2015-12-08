using System;
using System.Globalization;
using System.Net;
using System.Text;
using KinectPlayerTracker.Sockets;

namespace KinectPlayerTracker.TrackingConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IntializeTcpConnection();

            // 0x30 : stop     48
            // 0x31 : forward  49
            // 0x32 : left     50
            // 0x33 : backward 51
            // 0x34 : right    52

            while (true)
            {
                string input = Console.ReadLine();
                byte command = byte.Parse(input);
                _tcpClient.Send(command);
            }

            //Console.ReadKey();
        }

        private static AsyncTcpClient _tcpClient;

        private static void IntializeTcpConnection()
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
            _tcpClient.Connect();

            Console.WriteLine("TCP client is connecting to server.");
        }

        private static void SendData(byte[] data)
        {
            try
            {
                _tcpClient.Send(data);
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
