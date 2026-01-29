namespace Tftp
{
    /// <summary>
    /// Predefined error packets from RFC 1350
    /// </summary>
    enum TftpErrorCode : ushort
    {
        /// <summary>
        /// Not defined, see error message(if any).
        /// </summary>
        Undefined = 0,
        /// <summary>
        ///  File not found.
        /// </summary>
        FileNotFound = 1,
        /// <summary>
        /// Access violation.
        /// </summary>
        AccessViolation = 2,
        /// <summary>
        /// Disk full or allocation exceeded. 
        /// </summary>
        DiskFull = 3,
        /// <summary>
        /// Illegal Tftp operation.
        /// </summary>
        IllegalOperation = 4,
        /// <summary>
        /// Unknown transfer ID.
        /// </summary>
        UnknownTransferId = 5,
        /// <summary>
        /// File already exists.
        /// </summary>
        FileAlreadyExists = 6,
        /// <summary>
        /// No such user.
        /// </summary>
        NoSuchUser = 7,
        /// <summary>
        /// the request has been denied.
        /// </summary>
        RequestDenied = 8,
    }
}
