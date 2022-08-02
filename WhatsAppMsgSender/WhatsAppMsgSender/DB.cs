using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppMsgSender
{
    internal class DB
    {
        public static List<Message> GetDate()
        {
            List<Message> data = new List<Message>();
            data.Add(new Message { MSGType = "text" , Receipent = "311 xxxxxxx",Text = "hello this text is from automated app" });
            data.Add(new Message { MSGType = "file", Receipent = "311 xxxxxxx", Path = @"D:\test.txt" });
            data.Add(new Message { MSGType = "text" , Receipent = "311 xxxxxxx",Text = "hello this text is from automated app" });
            data.Add(new Message { MSGType = "file", Receipent = "311 xxxxxxx",Path = @"D:\test.txt" });
            data.Add(new Message { MSGType = "text", Receipent = "311 xxxxxxx", Text = "hello this text is from automated app" });
            data.Add(new Message { MSGType = "file", Receipent = "311 xxxxxxx", Path = @"D:\test.txt" });
            data.Add(new Message { MSGType = "text", Receipent = "311 xxxxxxx", Text = "hello this text is from automated app" });
            data.Add(new Message { MSGType = "file", Receipent = "311 xxxxxxx", Path = @"D:\test.txt" });
            data.Add(new Message { MSGType = "text", Receipent = "311 xxxxxxx", Text = "hello this text is from automated app" });
            data.Add(new Message { MSGType = "file", Receipent = "311 xxxxxxx", Path = @"D:\test.txt" });
            data.Add(new Message { MSGType = "text", Receipent = "311 xxxxxxx", Text = "hello this text is from automated app" });

            return data;
        }
    }
    public class Message
    {
        public string MSGType { get; set; }
        public string Receipent { get; set; }
        public string Text { get; set; }
        public string Path { get; set; }
    }
}
