namespace Tftp
{
    public enum TftpOperation
    {
        Upload,
        Download
    }

    public class TftpLaunchConfiguration
    {
        public string LocalIp { get; set; } = "0.0.0.0";
        public string RemoteIp { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 69;
        public ushort BlockSize { get; set; } = TftpConfiguration.DEFAULT_BLOCK_SIZE;
        public byte TimeoutSeconds { get; set; } = TftpConfiguration.DEFAULT_TIMEOUT_SECS;
        public TftpOperation Operation { get; set; }
        public string LocalFile { get; set; }
        public string RemoteFile { get; set; }
        public TftpLaunchConfiguration()
        {

        }

        public override string ToString()
        {
            return $"{Operation} file [{RemoteFile}] from {RemoteIp}:{RemotePort} to {LocalFile}";
        }
    }
}
