using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using ChatClient.Net.IO;
using System.Windows;
using System.IO;
using ChatClient.MVVM.Model;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using ChatClient.Helpers;
using System.ComponentModel;

namespace ChatClient.Net
{
    internal class Server : INotifyPropertyChanged
    {
        TcpClient _client;
        public PacketReader packetReader;
        public ObservableCollection<MessageModel> Messages { get; set; }


        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;
        public event Action fileReceivedEvent;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                            case 6:
                                Application.Current.Dispatcher.Invoke(() => fileReceivedEvent?.Invoke());
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
        public async Task SendFileToServer(string fileName, byte[] fileData)
        {
            try
            {
                var filePacket = new PacketBuilder();
                filePacket.WriteOpCode(6);
                filePacket.WriteMessage(fileName);
                filePacket.WriteInt32(fileData.Length);
                filePacket.WriteBytes(fileData);

                await _client.Client.SendAsync(filePacket.GetPacketBytes(), SocketFlags.None); // Gửi gói tin
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file: {ex.Message}", "File Send Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
