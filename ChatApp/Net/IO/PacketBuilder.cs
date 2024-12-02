using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    public class PacketBuilder
    {
        private MemoryStream _stream;

        public PacketBuilder()
        {
            _stream = new MemoryStream();
        }

        public void WriteOpCode(byte opcode)
        {
            _stream.WriteByte(opcode);
        }

        public void WriteMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _stream.Write(BitConverter.GetBytes(messageBytes.Length), 0, 4);
            _stream.Write(messageBytes, 0, messageBytes.Length);
        }

        public void WriteInt(int value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteBytes(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
        }

        public byte[] GetPacketBytes()
        {
            return _stream.ToArray();
        }
    }
}
