// Models/MessageDisplay.cs
using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;

namespace MauiApp1ChatWithAI.Models
{
    public class MessageDisplay
    {
        public string Role { get; set; }
        public List<MessagePart> Parts { get; set; } // メッセージのパーツをリストで保持
        public DateTime Timestamp { get; set; }

        public MessageDisplay(Message message)
        {
            Role = message.Role;
            Timestamp = message.Timestamp;
            Parts = new List<MessagePart>();
            foreach (var element in message.MessageElements)
            {
                Parts.Add(new MessagePart
                {
                    Type = element.Type,
                    Content = element.Content
                });
            }
        }
    }

    public class MessagePart
    {
        public string Type { get; set; } // "Text" または "Code"
        public string Content { get; set; }
    }
}