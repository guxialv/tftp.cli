using System;

namespace Tftp
{
    public class TftpTransferErrorEventArgs : EventArgs
    {
        public string Message { get; }
        public Exception Exception { get; }
        public TftpTransferErrorEventArgs(string message, Exception exception = null)
        {
            Message = message;
            Exception = exception;
        }
    }
}
