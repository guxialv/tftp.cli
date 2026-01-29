namespace Tftp
{
    public class TftpConfiguration
    {
        public const ushort DEFAULT_BLOCK_SIZE = 512;
        public const ushort MAX_BLOCK_SIZE = 65464;
        public const ushort MIN_BLOCK_SIZE = 8;

        public const byte DEFAULT_TIMEOUT_SECS = 5;
        public const byte MAX_TIMEOUT_SECS = 255;
        public const byte MIN_TIMEOUT_SECS = 1;

        public string LocalIp { get; set; } = "0.0.0.0";
        public string RemoteIp { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 69;
        public ushort BlockSize { get; set; } = DEFAULT_BLOCK_SIZE;
        public byte TimeoutSeconds { get; set; } = DEFAULT_TIMEOUT_SECS;

        public TftpConfiguration()
        {

        }
    }
}
