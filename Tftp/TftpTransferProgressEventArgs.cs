using System;

namespace Tftp
{
    public class TftpTransferProgressEventArgs : EventArgs
    {
        public long TransferredBlocks { get; }
        public long TransferredBytes { get; }
        public long TotalBytes { get; }

        public TftpTransferProgressEventArgs(long transferredBlocks, long transferredBytes, long totalBytes)
        {
            TransferredBlocks = transferredBlocks;
            TransferredBytes = transferredBytes;
            TotalBytes = totalBytes;
        }

    }
}
