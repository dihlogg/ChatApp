using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public List<string> _imageUrls;

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

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

            ConnectToServerCommand = new RelayCommand(async o => await _server.ConnectToServer(Username),
                                                       o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(async o => await _server.SendMessageToServer(Message),
                                                  o => !string.IsNullOrEmpty(Message));
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

        private void MessageReceived()
        {
            var msg = _server.packetReader.ReadMessage();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(new MessageModel { Content = msg, IsSentByMe = false });
            });
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
