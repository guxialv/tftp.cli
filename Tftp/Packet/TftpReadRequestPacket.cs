using System.Collections.Generic;

namespace Tftp
{
    sealed class TftpReadRequestPacket : TftpRequestPacket
    {
        public override TftpOpCode OpCode => TftpOpCode.RRQ;
        public TftpReadRequestPacket(string fileName, TftpTransferMode mode, Dictionary<string, string> options) :
            base(fileName, mode, options)
        {

        }

        public TftpReadRequestPacket()
            : base()
        {

        }
    }
}
