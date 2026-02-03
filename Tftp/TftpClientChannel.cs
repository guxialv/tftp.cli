using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Tftp
{
    public class TftpClientChannel : ITftpClientChannel
    {
        private bool _isServerReady;
        private TftpConfiguration _config;
        public bool IsServerReady => _isServerReady;
        public TftpClientChannel(string remoteIp) :
            this(new TftpConfiguration() { RemoteIp = remoteIp })
        {

        }

        public TftpClientChannel(TftpConfiguration config)
        {
            _config = config;
            _isServerReady = true;
        }

        public bool Download(string remoteFileName, string localFullFileName, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default)
        {
            if (File.Exists(localFullFileName))
            {
                File.Delete(localFullFileName);
            }

            using (var stream = new FileStream(localFullFileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                return Download(remoteFileName, stream, mode, token);
            }
        }

        public bool Download(string remoteFileName, Stream targetStream, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default)
        {
            using (var transfer = new TftpLocalReadTransfer(_config, targetStream))
            {
                transfer.Progress += OnProgress;
                transfer.Error += OnError;
                // Start() is synchronous and blocks until transfer completes
                // All events are raised synchronously, so no race condition when unsubscribing
                var result = transfer.Start(remoteFileName, mode, token);
                transfer.Progress -= OnProgress;
                transfer.Error -= OnError;

                return result;
            }
        }


        public bool Upload(string remoteFileName, string localFullFileName, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default)
        {
            using (var stream = new FileStream(localFullFileName, FileMode.Open, FileAccess.Read))
            {
                return Upload(remoteFileName, stream, mode, token);
            }
        }

        public bool Upload(string remoteFileName, Stream localFileStream, TftpTransferMode mode = TftpTransferMode.octet, CancellationToken token = default)
        {
            using (var transfer = new TftpLocalWriteTransfer(_config, localFileStream))
            {
                transfer.Progress += OnProgress;
                transfer.Error += OnError;
                // Start() is synchronous and blocks until transfer completes
                // All events are raised synchronously, so no race condition when unsubscribing
                var result = transfer.Start(remoteFileName, mode, token);
                transfer.Progress -= OnProgress;
                transfer.Error -= OnError;

                return result;
            }
        }

        public event EventHandler<TftpTransferProgressEventArgs> Progress;
        private void OnProgress(object sender, TftpTransferProgressEventArgs e)
        {
            Progress?.Invoke(this, e);
        }

        public event EventHandler<TftpTransferErrorEventArgs> Error;
        private void OnError(object sender, TftpTransferErrorEventArgs e)
        {
            if (e.Exception is SocketException ex && ex.SocketErrorCode != SocketError.Success)
            {
                _isServerReady = false;
            }
            Error?.Invoke(this, e);
        }

    }
}
