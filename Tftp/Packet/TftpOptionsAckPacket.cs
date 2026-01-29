using System.Collections.Generic;

namespace Tftp
{
    sealed class TftpOptionsAckPacket : TftpPacket
    {
        public override TftpOpCode OpCode => TftpOpCode.OACK;
        public Dictionary<string, string> Options { get; private set; }

        public TftpOptionsAckPacket(Dictionary<string, string> options)
            : base()
        {
            Options = options;
        }

        public TftpOptionsAckPacket()
            : base()
        {

        }

        public override void Serialize(TftpStream stream)
        {
            base.Serialize(stream);
            stream.WriteTransferOptions(Options);
        }
        public override void Deserialize(TftpStream stream)
        {
            base.Deserialize(stream);
            Options = stream.ReadTransferOptions();

        }
    }
}
