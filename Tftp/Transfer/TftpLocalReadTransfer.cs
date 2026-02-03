using System.IO;

namespace Tftp
{
    sealed class TftpLocalReadTransfer : TftpTransfer
    {
        private ushort _blockNumber;
        public TftpLocalReadTransfer(TftpConfiguration config, Stream targetStream) :
            base(config, targetStream, new TftpLocalReadTransferStateMachine())
        {
            _blockNumber = 0;
        }

        protected override void OnRequest(string remoteFileName, TftpTransferMode mode)
        {
            var transferSize = this._options.TransferSize = 0;
            var options = TftpTransferOptions.ToDictionary(_config.BlockSize, _config.TimeoutSeconds, transferSize);
            Exec(TftpCommandCode.ReadRequest, new TftpReadRequestPacket(remoteFileName, mode, options));
        }

        protected override void OnDataTransferring()
        {
            Exec(TftpCommandCode.Acknowledgment, new TftpAckPacket(_blockNumber));
        }

        protected override void OnDataTransferCompleted()
        {
            _connection.Send(new TftpAckPacket(_blockNumber)); // send last ack
        }

        protected override void OnResponseProcess(TftpPacket packet)
        {
            switch (packet)
            {
                case TftpOptionsAckPacket oack:
                    OnOptionsNegotiating(oack);
                    break;
                case TftpDataPacket data:
                    OnDataReceived(data);
                    break;
                case TftpErrorPacket error:
                    OnError(error);
                    break;
            }
        }

        private void OnDataReceived(TftpDataPacket data)
        {
            // Ignore duplicate or out-of-order packets
            if (unchecked((ushort)(_blockNumber + 1)) != data.BlockNumber)
            {
                return;
            }

            if (data.Count > 0)
            {
                _stream.Write(data.Data, data.Offset, data.Count);
            }

            _blockNumber = data.BlockNumber;
            _transferredBlocks++;
            _transferredBytes += data.Count;

            if (data.Count < _options.BlockSize /*|| _transferredBytes >= _options.TransferSize*/)
            {
                _isCompleted = true;
            }

            OnProgress();
        }
    }
}
