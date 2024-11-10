using CommunityToolkit.Mvvm.ComponentModel;
using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Models
{
    public class MessageDisplay : ObservableObject
    {
        private readonly Message _message;

        public MessageDisplay(Message message)
        {
            _message = message;
        }

        public string Role => _message.Role;
        public string Content => string.Join("\n",
            _message.MessageElements.Select(e => e.Content));

        public DateTime Timestamp => _message.Timestamp;

        // UI用のヘルパープロパティ
        public bool IsUser => Role.Equals("user", StringComparison.OrdinalIgnoreCase);
        public bool IsAssistant => Role.Equals("assistant", StringComparison.OrdinalIgnoreCase);
        public bool IsSystem => Role.Equals("system", StringComparison.OrdinalIgnoreCase);
    }
}
