using MauiApp1ChatWithAI.Models.Database;
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
        Task<string> GetResponseAsync(
            string message,
            List<Message> conversationHistory,  // 履歴はChatServiceが管理
            string threadId,
            string? systemPrompt = null         // スレッドから取得
        );
        Task InitializeAsync(string apiKey);
        Task<bool> LoadApiKey();
        Task ClearApiKey();
        bool IsInitialized { get; }
    }
}
