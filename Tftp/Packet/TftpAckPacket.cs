namespace Tftp
{
    sealed class TftpAckPacket : TftpPacket
    {
        public override TftpOpCode OpCode => TftpOpCode.ACK;
        public ushort BlockNumber { get; private set; }

        public TftpAckPacket(ushort blockNumber)
            : base()
        {
            BlockNumber = blockNumber;
        }

        public TftpAckPacket()
            : base()
        {

        }

        public override void Serialize(TftpStream stream)
        {
            base.Serialize(stream);
            stream.WriteUInt16(BlockNumber);
        }

        public override void Deserialize(TftpStream stream)
        {
            base.Deserialize(stream);
            BlockNumber = stream.ReadUInt16();
        }
    }
}
