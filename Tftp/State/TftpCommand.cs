using System;

namespace Tftp
{
    class TftpCommand
    {
        public TftpPacket Parameter { get; set; }
        public Func<TftpPacket, TftpPacket> CommandAction { get; set; }
        public TftpCommandCode CommandCode { get; set; }
        public TftpCommand()
        {

        }
        public TftpPacket Execute()
        {
            var transmit = 0;
            do
            {
                transmit++;
                try
                {
                    return CommandAction.Invoke(Parameter);
                }
                catch (Exception)
                {
                    if (transmit >= CommandCode.Retransmit)
                    {
                        throw;
                    }
                }

            } while (true);
        }
    }
}
