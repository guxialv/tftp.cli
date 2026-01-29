using System.Collections.Generic;

namespace Tftp
{
    sealed class TftpWriteRequestPacket : TftpRequestPacket
    {
        public override TftpOpCode OpCode => TftpOpCode.WRQ;
        public TftpWriteRequestPacket(string fileName, TftpTransferMode mode, Dictionary<string, string> options) :
            base(fileName, mode, options)
        {

        }
        public TftpWriteRequestPacket()
            : base()
        {

        }
    }
}
