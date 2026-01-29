namespace Tftp
{
    class TftpCommandCode
    {
        public static readonly TftpCommandCode ReadRequest = new TftpCommandCode(TftpOpCode.RRQ, 1);
        public static readonly TftpCommandCode WriteRequest = new TftpCommandCode(TftpOpCode.WRQ, 1);
        public static readonly TftpCommandCode Data = new TftpCommandCode(TftpOpCode.DATA, 6);
        public static readonly TftpCommandCode Acknowledgment = new TftpCommandCode(TftpOpCode.ACK, 6);
        public static readonly TftpCommandCode Error = new TftpCommandCode(TftpOpCode.ERROR, 1);
        public static readonly TftpCommandCode OptionAcknowledgment = new TftpCommandCode(TftpOpCode.OACK, 6);


        public TftpOpCode Code { get; }

        public byte Retransmit { get; }

        public TftpCommandCode(TftpOpCode code, byte retransmit)
        {
            Code = code;
            Retransmit = retransmit;
        }


        public string Name
        {
            get
            {
                var name = string.Empty;
                switch (Code)
                {
                    case TftpOpCode.RRQ:
                        name = nameof(ReadRequest);
                        break;
                    case TftpOpCode.WRQ:
                        name = nameof(WriteRequest);
                        break;
                    case TftpOpCode.DATA:
                        name = nameof(Data);
                        break;
                    case TftpOpCode.ACK:
                        name = nameof(Acknowledgment);
                        break;
                    case TftpOpCode.ERROR:
                        name = nameof(Error);
                        break;
                    case TftpOpCode.OACK:
                        name = nameof(OptionAcknowledgment);
                        break;
                    default:
                        break;
                }
                return name;
            }
        }


        public bool Equals(TftpCommandCode other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TftpCommandCode);
        }
        public override int GetHashCode()
        {
            return (ushort)Code;
        }

        public static bool operator ==(TftpCommandCode left, TftpCommandCode right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                {
                    // null == null = true
                    return true;
                }
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(TftpCommandCode left, TftpCommandCode right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
