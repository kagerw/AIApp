using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Models.Database
{
    public class ChatThread
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public string Provider { get; set; }  // デフォルト "Claude"
        public string? SystemPrompt { get; set; }
        public bool IsSystemPromptEnabled { get; set; }
    }
}
