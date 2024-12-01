using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Server.Net.IO;

namespace Server.Net
{
    public class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        public string IPAddress { get; set; }
        private PacketReader _packetReader;
        private MainWindow _mainWindow;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());
            _mainWindow = (MainWindow)Application.Current.MainWindow;

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            IPAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow.BroadcastMessage($"New client connected from IP Address {IPAddress} with {Username}");
            });

            Task.Run(() => Process());
        }

        private void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _mainWindow.BroadcastMessage($"[{DateTime.Now:HH:mm:ss}] [{IPAddress}] [{Username}]: {msg}");
                            });
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.BroadcastDisconnect(UID.ToString());
                    });
                    ClientSocket.Close();
                    break;
                }
            }
        }


    }
}
