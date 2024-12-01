using Server.Net.IO;
using Server.Net;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Server.MVVM.Model;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Client> _users;
        private TcpListener _listener;
        private bool _isServerRunning;

        public MainWindow()
        {
            InitializeComponent();
            _users = new ObservableCollection<Client>();
            UserListBox.ItemsSource = _users;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isServerRunning)
            {
                try
                {
                    _isServerRunning = true;
                    Dispatcher.Invoke(() =>
                    {
                        StartButton.Content = "Stop Server";
                    });
                    _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
                    _listener.Start();

                    AddToLog("Server started on 127.0.0.1:5000");

                    while (_isServerRunning)
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        var newClient = new Client(client);

                        // Logging chỉ một lần khi client kết nối
                        Dispatcher.Invoke(() =>
                        {
                            _users.Add(newClient);
                            //AddToLog($"Client connected from IP {((IPEndPoint)newClient.ClientSocket.Client.RemoteEndPoint).Address} with username is {newClient.Username}");
                            BroadcastConnection();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddToLog($"Error accepting client: {ex.Message}");
                        StopServer();
                    });
                }
            }
            else
            {
                StopServer();
            }
        }

        private async Task ListenForClients()
        {
            while (_isServerRunning)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();
                    var client = new Client(tcpClient);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _users.Add(client);
                        AddToLog($"New client connected with user name is {client.Username}");
                        BroadcastConnection();
                    });
                }
                catch (Exception ex)
                {
                    if (_isServerRunning)
                    {
                        AddToLog($"Error accepting client: {ex.Message}");
                    }
                }
            }
        }

        private void StopServer()
        {
            _isServerRunning = false;
            StartButton.Content = "Start Server";

            foreach (var user in _users)
            {
                try
                {
                    user.ClientSocket.Close();
                }
                catch { }
            }

            _users.Clear();
            _listener?.Stop();
            AddToLog("Server stopped");
        }

        private void BroadcastConnection()
        {
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    try
                    {
                        // Broadcast thông tin kết nối đến tất cả client khác
                        var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(1);
                        broadcastPacket.WriteMessage(usr.Username);
                        broadcastPacket.WriteMessage(usr.UID.ToString());
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                    }
                    catch (Exception ex)
                    {
                        AddToLog($"Error broadcasting connection: {ex.Message}");
                    }
                }
            }
        }


        public void BroadcastMessage(string message)
        {
            // Chỉ log một lần
            AddToLog(message);

            foreach (var user in _users)
            {
                try
                {
                    var msgPacket = new PacketBuilder();
                    msgPacket.WriteOpCode(5);
                    msgPacket.WriteMessage(message);
                    user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
                }
                catch (Exception ex)
                {
                    AddToLog($"Error broadcasting message: {ex.Message}");
                }
            }
        }

        public void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.FirstOrDefault(x => x.UID.ToString() == uid);
            if (disconnectedUser != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _users.Remove(disconnectedUser);
                    AddToLog($"[{((IPEndPoint)disconnectedUser.ClientSocket.Client.RemoteEndPoint).Address}] [{disconnectedUser.Username}] Disconnected!");
                });

                foreach (var user in _users)
                {
                    try
                    {
                        var broadcastPacket = new PacketBuilder();
                        broadcastPacket.WriteOpCode(10);
                        broadcastPacket.WriteMessage(uid);
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                    }
                    catch (Exception ex)
                    {
                        AddToLog($"Error broadcasting disconnect: {ex.Message}");
                    }
                }

                BroadcastMessage($"[{disconnectedUser.Username}] Disconnected!");
            }
        }

        private void AddToLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatLogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
                ChatLogTextBox.ScrollToEnd();
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopServer();
            base.OnClosing(e);
        }
    }
}