using System;
using System.Net;

namespace Tftp
{
    interface ITftpConnection : IDisposable
    {
        IPEndPoint RemoteEndPoint { get; }
        int TimeoutSeconds { set; }

        ITftpConnection Send(TftpPacket packet);
        TftpPacket Receive();
    }
}