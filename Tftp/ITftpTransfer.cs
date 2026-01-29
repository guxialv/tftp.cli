using System;
using System.Threading;

namespace Tftp
{
    interface ITftpTransfer : IDisposable
    {
        event EventHandler<TftpTransferProgressEventArgs> Progress;
        event EventHandler<TftpTransferErrorEventArgs> Error;

        bool Start(string remoteFileName, TftpTransferMode mode, CancellationToken token);
    }
}
