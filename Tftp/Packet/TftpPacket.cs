using System;
using System.IO;

namespace Tftp
{
    abstract class TftpPacket
    {
        public abstract TftpOpCode OpCode { get; }

        public TftpPacket()
        {

        }
        public virtual void Serialize(TftpStream stream)
        {
            stream.WriteUInt16((ushort)OpCode);
        }

        public virtual void Deserialize(TftpStream stream)
        {
            //this.OpCode = (TftpOpCode)ReadUInt16(stream);
        }

        protected void ValidateCode(TftpStream stream)
        {
            if ((TftpOpCode)stream.ReadUInt16() != OpCode) throw new InvalidDataException();
        }

        public override string ToString()
        {
            return $"OpCode={OpCode}";
        }

        // Static /////////////////////////////////////////////////////////////////////////

        public static TftpPacket Parser(TftpStream stream)
        {
            TftpPacket packet = null;

            var opCode = (TftpOpCode)stream.ReadUInt16();
            switch (opCode)
            {
                case TftpOpCode.RRQ:
                    packet = new TftpReadRequestPacket();
                    break;
                case TftpOpCode.WRQ:
                    packet = new TftpWriteRequestPacket();
                    break;
                case TftpOpCode.DATA:
                    packet = new TftpDataPacket();
                    break;
                case TftpOpCode.ACK:
                    packet = new TftpAckPacket();
                    break;
                case TftpOpCode.ERROR:
                    packet = new TftpErrorPacket();
                    break;
                case TftpOpCode.OACK:
                    packet = new TftpOptionsAckPacket();
                    break;
                default: throw new Exception($"Invalid TftpOpCode: {(ushort)opCode}");

            }
            packet.Deserialize(stream);
            return packet;
        }
    }
}
