namespace Tftp
{
    /// <summary>
    /// Predefined OpCode from RFC 1350
    /// </summary>
    enum TftpOpCode : ushort
    {
        /// <summary>
        ///  Read request
        /// </summary>
        RRQ = 1,
        /// <summary>
        /// Write request
        /// </summary>
        WRQ = 2,
        /// <summary>
        /// Data
        /// </summary>
        DATA = 3,
        /// <summary>
        /// Acknowledgment
        /// </summary>
        ACK = 4,
        /// <summary>
        /// Error
        /// </summary>
        ERROR = 5,
        /// <summary>
        /// Option Acknowledgment
        /// </summary>
        OACK = 6
    }
}