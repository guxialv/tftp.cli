namespace Tftp
{
    sealed class TftpDataPacket : TftpPacket
    {
        public override TftpOpCode OpCode => TftpOpCode.DATA;
        public ushort BlockNumber { get; private set; }

        public byte[] Data { get; private set; }
        public int Offset { get; private set; }
        public int Count { get; private set; }
        public TftpDataPacket(ushort blockNumber, byte[] data, int offset, int count)
            : base()
        {
            BlockNumber = blockNumber;
            Data = data;
            Offset = offset;
            Count = count;
        }

        public TftpDataPacket()
            : base()
        {

        }
        public override void Serialize(TftpStream stream)
        {
            base.Serialize(stream);
            stream.WriteUInt16(BlockNumber);
            stream.WriteBytes(Data, Offset, Count);
        }

        public override void Deserialize(TftpStream stream)
        {
            base.Deserialize(stream);
            BlockNumber = stream.ReadUInt16();
            Data = stream.ReadBytes();
            Offset = 0;
            Count = Data.Length;
        }
    }
}
