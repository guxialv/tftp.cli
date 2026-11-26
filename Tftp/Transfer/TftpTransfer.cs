using System;
using System.IO;
using System.Threading;

namespace Tftp
{
    abstract class TftpTransfer : ITftpTransfer
    {
        private int? _tid;
        private byte _incorrectTIDTimes;

        private bool _isFault;
        private bool _isCanceled;

        protected bool IsCompleted { get; set; }
        protected long TransferredBytes { get; set; }
        protected long TransferredBlocks { get; set; }

        protected Stream Stream { get; }
        protected ITftpConnection Connection { get; }
        protected TftpConfiguration Config { get; }
        protected TftpTransferOptions Options { get; }
        protected TftpTransferStateMachine StateMachine { get; }

        public TftpTransfer(TftpConfiguration config, Stream stream, TftpTransferStateMachine stateMachine)
        {
            _tid = null;
            Config = config;
            Stream = stream;
            StateMachine = stateMachine;
            Connection = new TftpUdpClient(config);
            Options = new TftpTransferOptions();
        }

        public bool Start(string remoteFileName, TftpTransferMode mode, CancellationToken token)
        {
            _isFault = false;
            _isCanceled = false;
            IsCompleted = false;

            _incorrectTIDTimes = 0;

            TransferredBytes = 0;
            TransferredBlocks = 0;

            try
            {
                OnRequest(remoteFileName, mode);

                // Set TID
                _tid = Connection.RemoteEndPoint.Port;

                while (IsCompleted == false && _isFault == false)
                {
                    if (token.IsCancellationRequested)
                    {
                        OnDataTransferCanceled();
                        break;
                    }
                    OnDataTransferring();
                }

                if (IsCompleted)
                {
                    OnDataTransferCompleted();
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            return IsCompleted == true && _isFault == false && _isCanceled == false;
        }

        protected abstract void OnResponseProcess(TftpPacket packet);
        protected abstract void OnRequest(string remoteFileName, TftpTransferMode mode);
        protected abstract void OnDataTransferring();
        protected abstract void OnDataTransferCompleted();

        protected void OnDataTransferCanceled()
        {
            _isCanceled = true;
            Connection.Send(TftpErrorPacket.Undefined);
            OnError("Transfer canceled by user");
        }

        protected void OnOptionsNegotiating(TftpOptionsAckPacket oack)
        {
            Options.Negotiate(oack.Options);
        }

        protected void Exec(TftpCommandCode code, TftpPacket request)
        {
            var response = StateMachine.Exec(code, request, (req) => Connection.Send(req).Receive());

            if (ValidateTID() == false)
            {
                return;
            }
            OnResponseProcess(response);
        }

        private bool ValidateTID()
        {
            var isValid = _tid == null || _tid == Connection.RemoteEndPoint.Port;
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
            Progress?.Invoke(this, new TftpTransferProgressEventArgs(TransferredBlocks, TransferredBytes, Options.TransferSize));
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
                    Stream.Dispose();
                    Connection.Dispose();
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
