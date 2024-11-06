using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Models.Database
{
    public class Message
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
        public string Role { get; set; }  // "user" or "assistant"
        public DateTime Timestamp { get; set; }
        public virtual ICollection<MessageElement> MessageElements { get; set; } = new List<MessageElement>();
    }
}
