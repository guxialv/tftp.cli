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
            _buffer = new byte[config.BlockSize * 2];
        }

        protected override void OnRequest(string remoteFileName, TftpTransferMode mode)
        {
            var transferSize = this._options.TransferSize = _stream.Length;
            var options = TftpTransferOptions.ToDictionary(_config.BlockSize, _config.TimeoutSeconds, transferSize);
            Exec(TftpCommandCode.WriteRequest, new TftpWriteRequestPacket(remoteFileName, mode, options));
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
                _connection.Send(dataPacket);// Send last block
            }
        }
        protected override void OnResponseProcess(TftpPacket packet)
        {
            if (packet is TftpOptionsAckPacket oack)
            {
                OnOptionsNegotiating(oack);
            }
            else if (packet is TftpAckPacket ack)
            {
                OnAck(ack);
            }
            else if (packet is TftpErrorPacket error)
            {
                OnError(error);
            }
        }


        private void OnAck(TftpAckPacket ack)
        {
            _blockNumber = ack.BlockNumber;
        }

        private TftpDataPacket ReadNextBlock()
        {
            if (_buffer.Length < _options.BlockSize)
            {
                _buffer = new byte[_options.BlockSize * 2];
            }

            var count = _stream.Read(_buffer, 0, _options.BlockSize);
            _blockNumber++;
            _transferredBlocks++;
            _transferredBytes += count;

            if (count < _options.BlockSize || _transferredBytes >= _options.TransferSize)
            {
                _isCompleted = true;
                _sendLastPack = count == _options.BlockSize;
            }

            OnProgress();

            var dataPacket = new TftpDataPacket(_blockNumber, _buffer, 0, count);
            return dataPacket;
        }
    }
}
