using System;
using System.IO;
using System.Threading;

namespace Tftp
{
    abstract class TftpTransfer : ITftpTransfer
    {
        protected int? _tid;
        protected byte _incorrectTIDTimes;

        protected bool _isFault;
        protected bool _isCanceled;
        protected bool _isCompleted;

        protected long _transferredBytes;
        protected long _transferredBlocks;

        protected Stream _stream { get; }
        protected ITftpConnection _connection { get; }
        protected TftpConfiguration _config { get; }
        protected TftpTransferOptions _options { get; }
        protected TftpTransferStateMachine _stateMachine { get; }

        public TftpTransfer(TftpConfiguration config, Stream stream, TftpTransferStateMachine stateMachine)
        {
            _tid = null;
            _config = config;
            _stream = stream;
            _stateMachine = stateMachine;
            _connection = new TftpUdpClient(config);
            _options = new TftpTransferOptions();
        }

        public bool Start(string remoteFileName, TftpTransferMode mode, CancellationToken token)
        {
            _isFault = false;
            _isCanceled = false;
            _isCompleted = false;

            _incorrectTIDTimes = 0;

            _transferredBytes = 0;
            _transferredBlocks = 0;

            try
            {
                OnRequest(remoteFileName, mode);

                // Set TID
                _tid = _connection.RemoteEndPoint.Port;

                while (_isCompleted == false && _isFault == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        OnDataTransferCanceled();
                        break;
                    }
                    OnDataTransferring();
                }

                if (_isCompleted)
                {
                    OnDataTransferCompleted();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            return _isCompleted == true && _isFault == false && _isCanceled == false;
        }

        protected abstract void OnResponseProcess(TftpPacket packet);
        protected abstract void OnRequest(string remoteFileName, TftpTransferMode mode);
        protected abstract void OnDataTransferring();
        protected abstract void OnDataTransferCompleted();

        protected void OnDataTransferCanceled()
        {
            _isCanceled = true;
            _connection.Send(TftpErrorPacket.Undefined);
            OnError("Transfer canceled by user");
        }

        protected void OnOptionsNegotiating(TftpOptionsAckPacket oack)
        {
            _options.Negotiate(oack.Options);
            _connection.TimeoutSeconds = _options.Timeout;
        }

        protected void Exec(TftpCommandCode code, TftpPacket request)
        {
            var response = _stateMachine.Exec(code, request, (req) => _connection.Send(req).Receive());

            if (ValidateTID() == false)
            {
                return;
            }
            OnResponseProcess(response);
        }

        private bool ValidateTID()
        {
            var isValid = _tid == null || _tid == _connection.RemoteEndPoint.Port;
            if (isValid == false)
            {
                _incorrectTIDTimes++;
            }
            else
            {
                _incorrectTIDTimes = 0;
            }

            if (_incorrectTIDTimes > 100)
            {
                OnError("Incorrect TID");
            }
            return isValid;
        }


        public event EventHandler<TftpTransferProgressEventArgs> Progress;
        protected void OnProgress()
        {
            Progress?.Invoke(this, new TftpTransferProgressEventArgs(_transferredBlocks, _transferredBytes, _options.TransferSize));
        }

        public event EventHandler<TftpTransferErrorEventArgs> Error;
        protected void OnError(string error)
        {
            _isFault = true;
            Error?.Invoke(this, new TftpTransferErrorEventArgs(error));
        }

        protected void OnError(Exception ex)
        {
            _isFault = true;
            Error?.Invoke(this, new TftpTransferErrorEventArgs(ex.Message, ex));
        }
        protected void OnError(TftpErrorPacket error)
        {
            _isFault = true;
            Error?.Invoke(this, new TftpTransferErrorEventArgs(error.ErrorMessage, error.Exception));
        }

        private bool _isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _stream.Dispose();
                    _connection.Dispose();
                }

                _isDisposed = true;
            }
        }

        ~TftpTransfer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
