using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Server.MVVM.Model;
using Server.Net.IO;

namespace Server.Net
{
    public class Client
    {
        public string Username { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        public string IPAddress { get; set; }
        private PacketReader _packetReader;
        private MainWindow _mainWindow;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            Messages = new ObservableCollection<MessageModel>();
            _packetReader = new PacketReader(ClientSocket.GetStream());
            _mainWindow = (MainWindow)Application.Current.MainWindow;

            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            IPAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow.BroadcastMessage($"New client connected from IP Address {IPAddress} with Username {Username}");
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
                                _mainWindow.BroadcastMessage($"[{IPAddress}] [{Username}]: {msg}");
                            });
                            break;
                        case 6:
                            //Nhận file từ client
                        string fileName = _packetReader.ReadMessage();
                            int fileSize = _packetReader.ReadInt32();

                            if (fileSize <= 0 || fileSize > 100 * 1024 * 1024)
                                throw new InvalidDataException($"Invalid file size: {fileSize}");

                            byte[] fileData = _packetReader.ReadBytes(fileSize);

                            // Lưu file vào thư mục tạm
                            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
                            File.WriteAllBytes(tempFilePath, fileData);

                            bool isImage = IsImageFile(tempFilePath);

                            // Thêm thông báo vào giao diện
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _mainWindow.Messages.Add(new MessageModel
                                {
                                    Content = isImage ? "Image Received" : $"File Received: {fileName}",
                                    IsFile = true,
                                    FilePath = tempFilePath,
                                    IsSentByMe = false
                                });
                            });

                            // Gửi file đến các client khác
                            _mainWindow.BroadcastFile(fileName, fileData, Username);
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

        private bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            string extension = Path.GetExtension(filePath)?.ToLower();
            return imageExtensions.Contains(extension);
        }
    }
}
