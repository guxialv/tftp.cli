using System;

namespace Tftp
{
    sealed class TftpErrorPacket : TftpPacket
    {
        public static readonly TftpErrorPacket Undefined = new TftpErrorPacket(TftpErrorCode.Undefined, "Unknown error");
        public static readonly TftpErrorPacket FileNotFound = new TftpErrorPacket(TftpErrorCode.FileNotFound, "File not found");
        public static readonly TftpErrorPacket AccessViolation = new TftpErrorPacket(TftpErrorCode.AccessViolation, "Access violation");
        public static readonly TftpErrorPacket DiskFull = new TftpErrorPacket(TftpErrorCode.DiskFull, "Disk full or allocation exceeded");
        public static readonly TftpErrorPacket IllegalOperation = new TftpErrorPacket(TftpErrorCode.IllegalOperation, "Illegal TFTP operation");
        public static readonly TftpErrorPacket UnknownTransferId = new TftpErrorPacket(TftpErrorCode.UnknownTransferId, "Unknown transfer ID");
        public static readonly TftpErrorPacket FileAlreadyExists = new TftpErrorPacket(TftpErrorCode.FileAlreadyExists, "File already exists");
        public static readonly TftpErrorPacket NoSuchUser = new TftpErrorPacket(TftpErrorCode.NoSuchUser, "No such user");
        public static readonly TftpErrorPacket RequestDenied = new TftpErrorPacket(TftpErrorCode.RequestDenied, "The request has been denied");

        public override TftpOpCode OpCode => TftpOpCode.ERROR;
        public TftpErrorCode ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public Exception Exception { get; private set; }

        public TftpErrorPacket(TftpErrorCode errorCode, string errorMessage)
            : base()
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public TftpErrorPacket(Exception exception) :
            this(TftpErrorCode.Undefined, exception.Message)
        {
            Exception = exception;
        }

        public TftpErrorPacket()
            : base()
        {

        }

        public override void Serialize(TftpStream stream)
        {
            base.Serialize(stream);
            stream.WriteUInt16((ushort)ErrorCode);
            stream.WriteNullTerminatedString(ErrorMessage);
        }

        public override void Deserialize(TftpStream stream)
        {
            base.Deserialize(stream);
            ErrorCode = (TftpErrorCode)stream.ReadUInt16();
            ErrorMessage = stream.ReadNullTerminatedString();
        }

    }
}
