using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace ChatClient.Net.IO
{
    internal class PacketBuilder
    {
        MemoryStream _ms;
        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        public void WriteMessage(string msg)
        {
            var msgEncoding = Encoding.UTF8.GetBytes(msg);
            _ms.Write(BitConverter.GetBytes(msgEncoding.Length));
            _ms.Write(msgEncoding);
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
