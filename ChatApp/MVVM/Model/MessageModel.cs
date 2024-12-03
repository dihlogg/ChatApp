using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ChatClient.MVVM.Model
{
    public class MessageModel : INotifyPropertyChanged
    {
        public string Content { get; set; }
        public bool IsSentByMe { get; set; }
        public string Sender { get; set; }
        public bool IsFile { get; set; }
        public string FilePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private BitmapImage _bitmapImage;
        public BitmapImage bitmapImage
        {
            get => _bitmapImage;
            set
            {
                if (_bitmapImage != value)
                {
                    _bitmapImage = value;
                    OnPropertyChanged(nameof(bitmapImage));
                }
            }
        }
    }
}
