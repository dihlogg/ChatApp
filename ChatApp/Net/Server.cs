using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using ChatClient.Net.IO;
using System.Windows;

namespace ChatClient.Net
{
    internal class Server
    {
        TcpClient _client;
        public PacketReader packetReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;

        public Server()
        {
            _client = new TcpClient();
        }

        public async Task ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                try
                {
                    await _client.ConnectAsync("127.0.0.1", 5000);
                    packetReader = new PacketReader(_client.GetStream());

                    if (!string.IsNullOrEmpty(username))
                    {
                        var connectPacket = new PacketBuilder();
                        connectPacket.WriteOpCode(0);
                        connectPacket.WriteMessage(username);
                        await _client.Client.SendAsync(connectPacket.GetPacketBytes(), SocketFlags.None);
                    }
                    ReadPackets();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to connect to server: {ex.Message}", "Connection Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var opcode = packetReader.ReadByte();
                        switch (opcode)
                        {
                            case 1:
                                Application.Current.Dispatcher.Invoke(() => connectedEvent?.Invoke());
                                break;
                            case 5:
                                Application.Current.Dispatcher.Invoke(() => msgReceivedEvent?.Invoke());
                                break;
                            case 10:
                                Application.Current.Dispatcher.Invoke(() => userDisconnectEvent?.Invoke());
                                break;
                            default:
                                Console.WriteLine("Unknown opcode received");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading packets: {ex.Message}");
                        break;
                    }
                }
            });
        }

        public async Task SendMessageToServer(string message)
        {
            try
            {
                var messagePacket = new PacketBuilder();
                messagePacket.WriteOpCode(5);
                messagePacket.WriteMessage(message);
                await _client.Client.SendAsync(messagePacket.GetPacketBytes(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Send Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
