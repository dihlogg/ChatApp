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

namespace ChatClient.Net
{
    internal class Server
    {
        TcpClient _client;
        public PacketReader packetReader;
        public ObservableCollection<MessageModel> Messages { get; set; }


        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectEvent;
        public event Action fileReceivedEvent;

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
                                ReceiveFile();
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
                        break; // Thoát vòng lặp nếu có lỗi
                    }
                }
            });
        }
        public async Task SendFileToServer(string fileName, byte[] fileData)
        {
            try
            {
                var filePacket = new PacketBuilder();
                filePacket.WriteOpCode(6); // OpCode 6 dành cho gửi file
                filePacket.WriteMessage(fileName);
                filePacket.WriteBytes(fileData);

                await _client.Client.SendAsync(filePacket.GetPacketBytes(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file: {ex.Message}", "File Send Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceiveFile()
        {
            string fileName = packetReader.ReadMessage(); // Nhận tên file
            int fileSize = packetReader.ReadInt32(); // Nhận kích thước file
            byte[] fileData = packetReader.ReadBytes(fileSize); // Nhận dữ liệu file

            string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            File.WriteAllBytes(savePath, fileData); // Lưu file

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new MessageModel
                {
                    Content = $"[File Received: {fileName}]",
                    IsFile = true,
                    FilePath = savePath,
                    IsSentByMe = false
                });
            });

            MessageBox.Show($"File {fileName} received and saved to Desktop", "File Received", MessageBoxButton.OK, MessageBoxImage.Information);
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
