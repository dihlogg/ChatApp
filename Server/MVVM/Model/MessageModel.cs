using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.MVVM.Model
{
    public class MessageModel
    {
        public string Content { get; set; }
        public bool IsSentByMe { get; set; }
        public string Sender { get; set; }
        public bool IsFile { get; set; } // Nếu là file thì true
        public string FilePath { get; set; } // Đường dẫn file
    }
}
