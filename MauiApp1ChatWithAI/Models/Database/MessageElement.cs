using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Models.Database
{
    public class MessageElement
    {
        public string Id { get; set; }
        public string MessageId { get; set; }
        public string Type { get; set; }  // "Text", "Code", "Expression"
        public string Content { get; set; }
        public string? Language { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual Message Message { get; set; }
    }
}
