using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Net.IO
{
    public class PacketReader : BinaryReader
    {
        private NetworkStream _stream;

        public PacketReader(NetworkStream stream) : base(stream)
        {
            _stream = stream;
        }

        public string ReadMessage()
        {
            int length = ReadInt32();
            byte[] buffer = new byte[length];
            _stream.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        public int ReadInt32()
        {
            byte[] buffer = ReadBytes(4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public byte[] ReadBytes(int count)
        {
            if (count <= 0 || count > 100 * 1024 * 1024) // Giới hạn 100MB
                throw new ArgumentOutOfRangeException(nameof(count), "Invalid byte count requested");

            byte[] buffer = new byte[count];
            int bytesRead = 0;

            while (bytesRead < count)
            {
                int read = _stream.Read(buffer, bytesRead, count - bytesRead);
                if (read == 0)
                    throw new EndOfStreamException("Stream ended before reading all bytes");

                bytesRead += read;
            }

            return buffer;
        }
    }
}
