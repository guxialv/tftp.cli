using System.IO;

namespace Tftp
{
    sealed class TftpLocalWriteTransfer : TftpTransfer
    {
        private byte[] _buffer;
        private bool _sendLastPack;
        private ushort _blockNumber;
        public TftpLocalWriteTransfer(TftpConfiguration config, Stream localFileStream)
            : base(config, localFileStream, new TftpLocalWriteTransferStateMachine())
        {
            _blockNumber = 0;
            _sendLastPack = false;
            _buffer = new byte[config.BlockSize];
        }

        protected override void OnRequest(string remoteFileName, TftpTransferMode mode)
        {
            var transferSize = this.Options.TransferSize = Stream.Length;
            var options = TftpTransferOptions.ToDictionary(Config.BlockSize, Config.TimeoutSeconds, transferSize);
            Exec(TftpCommandCode.WriteRequest, new TftpWriteRequestPacket(remoteFileName, mode, options));
            Connection.TimeoutSeconds = Options.Timeout;
        }

        protected override void OnDataTransferring()
        {
            Exec(TftpCommandCode.Data, ReadNextBlock());
        }

        protected override void OnDataTransferCompleted()
        {
            if (_sendLastPack)
            {
                _blockNumber++;
                var dataPacket = new TftpDataPacket(_blockNumber, _buffer, 0, 0);
                Connection.Send(dataPacket);// Send last block
            }
        }

        protected override void OnResponseProcess(TftpPacket packet)
        {
            switch (packet)
            {
                case TftpOptionsAckPacket oack:
                    OnOptionsNegotiating(oack);
                    break;
                case TftpAckPacket ack:
                    OnAck(ack);
                    break;
                case TftpErrorPacket error:
                    OnError(error);
                    break;
            }
        }


        private void OnAck(TftpAckPacket ack)
        {
            _blockNumber = ack.BlockNumber;
        }

        private TftpDataPacket ReadNextBlock()
        {
            // Reallocate buffer only if block size changes during transfer
            if (_buffer.Length < Options.BlockSize)
            {
                _buffer = new byte[Options.BlockSize];
            }

            var count = Stream.Read(_buffer, 0, Options.BlockSize);
            _blockNumber++;
            TransferredBlocks++;
            TransferredBytes += count;

            if (count < Options.BlockSize /*|| TransferredBytes >= Options.TransferSize*/)
            {
                IsCompleted = true;
                _sendLastPack = count == Options.BlockSize;
            }

            OnProgress();

            var dataPacket = new TftpDataPacket(_blockNumber, _buffer, 0, count);
            return dataPacket;
        }
    }
}
