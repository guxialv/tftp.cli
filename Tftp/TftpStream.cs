
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tftp
{
    class TftpStream : IDisposable
    {
        private readonly MemoryStream _stream;

        public TftpStream()
        {
            _stream = new MemoryStream();
        }

        public TftpStream(byte[] bytes, int offset, int count)
        {
            _stream = new MemoryStream(bytes, offset, count, false, true);
        }

        // Write /////////////////////////////////////////////////////////////////////
        public void WriteUInt16(ushort value)
        {
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)(value & 0xFF));
        }

        public void WriteByte(byte b)
        {
            _stream.WriteByte(b);
        }


        public void WriteBytes(byte[] data, int offset, int count)
        {
            _stream.Write(data, offset, count);
        }

        public void WriteBytes(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public void WriteNullTerminatedString(string value)
        {
            WriteBytes(Encoding.ASCII.GetBytes(value));
            WriteByte(0);
        }

        public void WriteTransferOptions(Dictionary<string, string> options)
        {
            foreach (var option in options)
            {
                WriteNullTerminatedString(option.Key);
                WriteNullTerminatedString(option.Value);
            }
        }

        // Read /////////////////////////////////////////////////////////////////////
        public ushort ReadUInt16()
        {
            int byte1 = _stream.ReadByte();
            int byte2 = _stream.ReadByte();
            return (ushort)((byte)byte1 << 8 | (byte)byte2);
        }

        public byte ReadByte()
        {
            int nextByte = _stream.ReadByte();

            if (nextByte == -1)
                throw new IOException();

            return (byte)nextByte;
        }

        public byte[] ReadBytes()
        {
            long remainingBytes = _stream.Length - _stream.Position;
            
            // If no remaining bytes, return empty array
            if (remainingBytes <= 0)
                return Array.Empty<byte>();
            
            // Allocate buffer with exact size needed
            byte[] buffer = new byte[remainingBytes];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);

            if (bytesRead == -1)
                throw new IOException();

            // Only resize if we didn't read all expected bytes
            if (bytesRead < remainingBytes)
            {
                Array.Resize(ref buffer, bytesRead);
            }
            
            return buffer;
        }

        public string ReadNullTerminatedString()
        {
            byte b;
            // Pre-allocate reasonable capacity to reduce reallocations
            StringBuilder sb = new StringBuilder(32);
            while ((b = ReadByte()) > 0)
            {
                sb.Append((char)b);
            }

            return sb.ToString();
        }

        public Dictionary<string, string> ReadTransferOptions()
        {
            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            while (_stream.Position < _stream.Length)
            {
                var name = ReadNullTerminatedString().ToLower();
                var value = ReadNullTerminatedString().ToLower();
                options.Add(name, value);
            }
            return options;
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
