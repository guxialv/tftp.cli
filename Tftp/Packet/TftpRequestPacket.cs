using System;
using System.Collections.Generic;

namespace Tftp
{
    abstract class TftpRequestPacket : TftpPacket
    {
        public string FileName { get; private set; }
        public TftpTransferMode Mode { get; private set; }
        public Dictionary<string, string> Options { get; private set; }

        protected TftpRequestPacket(string fileName, TftpTransferMode mode, Dictionary<string, string> options)
            : base()
        {
            FileName = fileName;
            Mode = mode;
            Options = options;
        }

        public TftpRequestPacket()
            : base()
        {

        }

        public override void Serialize(TftpStream stream)
        {
            base.Serialize(stream);
            stream.WriteNullTerminatedString(FileName);
            stream.WriteNullTerminatedString(Mode.ToString());
            stream.WriteTransferOptions(Options);
        }

        public override void Deserialize(TftpStream stream)
        {
            base.Deserialize(stream);
            FileName = stream.ReadNullTerminatedString();
            Mode = (TftpTransferMode)Enum.Parse(typeof(TftpTransferMode), stream.ReadNullTerminatedString(), false);
            Options = stream.ReadTransferOptions();
        }
    }
}
