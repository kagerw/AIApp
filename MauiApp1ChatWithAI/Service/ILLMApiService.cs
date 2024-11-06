using MauiApp1ChatWithClaude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public interface ILLMApiService
    {
        Task<string> GetResponseAsync(string message);
        Task InitializeAsync(string apiKey);
        Task<bool> LoadApiKey();
        Task ClearApiKey();
        void ClearHistory();
        bool IsInitialized { get; }
        //IReadOnlyList<Message> ConversationHistory { get; }
    }
}
