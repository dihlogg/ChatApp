using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using ChatClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public List<string> _imageUrls;

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand SendFileCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }

        public string Username { get; set; }
        public string Message { get; set; }

        private readonly Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<MessageModel>();
            _imageUrls = new List<string>
            {
                "pack://application:,,,/Assets/2.jpg",
                "pack://application:,,,/Assets/3.jpg",
                "pack://application:,,,/Assets/4.jpg",
                "pack://application:,,,/Assets/5.jpg",
                "pack://application:,,,/Assets/6.jpg",
                "pack://application:,,,/Assets/7.jpg",
                "pack://application:,,,/Assets/8.jpg",
            };

            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectEvent += RemoveUser;

            SendFileCommand = new RelayCommand(async obj =>
            {
                if (obj is string filePath && !string.IsNullOrEmpty(filePath))
                {
                    await SendFile(filePath);
                }
            });

            OpenFileCommand = new RelayCommand(OpenFile);

            ConnectToServerCommand = new RelayCommand(async o => await _server.ConnectToServer(Username),
                                                       o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(async o => await _server.SendMessageToServer(Message),
                                                  o => !string.IsNullOrEmpty(Message));
        }

        private async Task SendFile(string filePath)
        {
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(filePath); // Đọc file thành mã nhị phân
                string fileName = Path.GetFileName(filePath);

                await _server.SendFileToServer(fileName, fileData); // Gửi file qua server

                // Cập nhật giao diện
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new MessageModel
                    {
                        Content = $"[File Sent: {fileName}]",
                        IsFile = true,
                        FilePath = filePath,
                        IsSentByMe = true
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file: {ex.Message}", "File Send Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void OpenFile(object obj)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                await SendFile(filePath);
            }
        }

        private async Task SendMessage()
        {
            await _server.SendMessageToServer(Message);
            Messages.Add(new MessageModel
            {
                Content = Message,
                IsSentByMe = true,
                IsFile = false
            });
            Message = string.Empty;
        }
        private void MessageReceived()
        {
            var msg = _server.packetReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new MessageModel { Content = msg, IsSentByMe = false });
            });
        }

        private void UserConnected()
        {
            var username = _server.packetReader.ReadMessage();
            var uid = _server.packetReader.ReadMessage();
            var user = new UserModel
            {
                Username = username,
                UID = uid,
                ImageUrl = GetNextImageUrl()
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        private void RemoveUser()
        {
            var uid = _server.packetReader.ReadMessage();
            var user = Users.FirstOrDefault(x => x.UID == uid);
            if (user != null)
            {
                Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
            }
        }

        private string GetNextImageUrl()
        {
            int index = Users.Count % _imageUrls.Count;
            return _imageUrls[index];
        }
    }
}
