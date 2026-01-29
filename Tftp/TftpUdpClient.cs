using System.Net;
using System.Net.Sockets;

namespace Tftp
{
    class TftpUdpClient : ITftpConnection
    {
        //const uint IOC_IN = 0x80000000;
        //const uint IOC_VENDOR = 0x18000000;
        //const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

        private EndPoint _LocalEndPoint;
        private EndPoint _RemoteEndPoint;
        private Socket _socket;
        private readonly byte[] _receiveBuffer;

        public IPEndPoint RemoteEndPoint
        {
            get => (IPEndPoint)_RemoteEndPoint;
        }

        public int TimeoutSeconds
        {
            set
            {
                if (_socket != null && value > 0)
                {
                    var timeout = value * 1000;
                    _socket.ReceiveTimeout = timeout;
                    _socket.SendTimeout = timeout;
                }
            }
        }

        public TftpUdpClient(TftpConfiguration settings)
        {
            _LocalEndPoint = new IPEndPoint(IPAddress.Parse(settings.LocalIp), IPEndPoint.MinPort);
            _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(settings.RemoteIp), settings.RemotePort);

            _socket = new Socket(_LocalEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(_LocalEndPoint);

            //// IOControl() is only available on windows. This call prevents the UDP socket from
            //// being closed as a result of ICMP Port Unreachable messages.
            //// see: https://stackoverflow.com/questions/15228272/what-would-cause-a-connectionreset-on-an-udp-socket
            //_socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            _socket.SendTimeout = 1 * 1000;        // this only affects synchronous Send
            _socket.ReceiveTimeout = 1 * 1000;     // this only affects synchronous Receive
            _receiveBuffer = new byte[TftpConfiguration.MAX_BLOCK_SIZE + 4];
        }


        public TftpPacket Receive()
        {
            var count = _socket.ReceiveFrom(_receiveBuffer, SocketFlags.None, ref _RemoteEndPoint);
            using (var stream = new TftpStream(_receiveBuffer, 0, count))
            {
                var packet = TftpPacket.Parser(stream);

                return packet;
            }
        }

        public ITftpConnection Send(TftpPacket packet)
        {
            using (var stream = new TftpStream())
            {
                packet.Serialize(stream);
                var buffer = stream.ToArray();
                _socket.SendTo(buffer, SocketFlags.None, _RemoteEndPoint);
            }
            return this;
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _socket = null;
        }
    }
}
