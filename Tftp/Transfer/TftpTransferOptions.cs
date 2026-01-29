using System;
using System.Collections.Generic;

namespace Tftp
{
    class TftpTransferOptions
    {
        private const string BLOCK_SIZE_OPTION = "blksize";
        private const string TIMEOUT_OPTION = "timeout";
        private const string TRANSFER_SIZE_OPTION = "tsize";

        public ushort BlockSize { get; set; }
        public byte Timeout { get; set; }
        public long TransferSize { get; set; }

        public TftpTransferOptions()
        {
            BlockSize = TftpConfiguration.DEFAULT_BLOCK_SIZE;
            Timeout = TftpConfiguration.DEFAULT_TIMEOUT_SECS;
            TransferSize = 0;
        }

        public void Negotiate(Dictionary<string, string> options)
        {
            BlockSize = NegotiateBlockSizeOption(options);
            Timeout = NegotiateTimeoutOption(options);
            TransferSize = NegotiateTransferSizeOption(options);
        }

        private ushort NegotiateBlockSizeOption(Dictionary<string, string> options)
        {
            if (options.TryGetValue(BLOCK_SIZE_OPTION, out var option) &&
                ushort.TryParse(option, out var blockSize)
                && ValidateBlockSize(blockSize))
            {

                return blockSize;
            }
            else
            {
                return TftpConfiguration.DEFAULT_BLOCK_SIZE;
            }
        }

        private byte NegotiateTimeoutOption(Dictionary<string, string> options)
        {
            if (options.TryGetValue(TIMEOUT_OPTION, out var option) &&
                byte.TryParse(option, out var timeout) &&
                ValidateTimeout(timeout))
            {
                return timeout;
            }
            else
            {
                return TftpConfiguration.DEFAULT_TIMEOUT_SECS;
            }
        }

        private long NegotiateTransferSizeOption(Dictionary<string, string> options)
        {
            if (options.TryGetValue(TRANSFER_SIZE_OPTION, out var option) &&
                long.TryParse(option, out var transferSize) &&
                ValidateTransferSize(transferSize))
            {
                return transferSize;
            }
            else
            {
                return 0;
            }
        }

        public static Dictionary<string, string> ToDictionary(ushort blockSize, byte timeout, long transferSize)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (ValidateBlockSize(blockSize))
            {
                dict[BLOCK_SIZE_OPTION] = blockSize.ToString();
            }
            if (ValidateTimeout(timeout))
            {
                dict[TIMEOUT_OPTION] = timeout.ToString();
            }
            if (ValidateTransferSize(transferSize))
            {
                dict[TRANSFER_SIZE_OPTION] = transferSize.ToString();
            }
            return dict;
        }

        private static bool ValidateBlockSize(ushort value) =>
            value >= TftpConfiguration.MIN_BLOCK_SIZE && value <= TftpConfiguration.MAX_BLOCK_SIZE;
        private static bool ValidateTimeout(byte value) =>
            value >= TftpConfiguration.MIN_TIMEOUT_SECS && value <= TftpConfiguration.MAX_TIMEOUT_SECS;
        private static bool ValidateTransferSize(long value) =>
         value >= 0;

    }
}
