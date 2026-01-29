using System;
using System.IO;
using System.Threading;

namespace Tftp
{
    public interface ITftpClientChannel
    {
        bool IsServerReady { get; }
        event EventHandler<TftpTransferErrorEventArgs> Error;
        event EventHandler<TftpTransferProgressEventArgs> Progress;

        bool Download(string remoteFileName, Stream targetStream, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default);
        bool Download(string remoteFileName, string localFullFileName, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default);
        bool Upload(string remoteFileName, Stream localFileStream, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default);
        bool Upload(string remoteFileName, string localFullFileName, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default);
    }
}