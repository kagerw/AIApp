using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public interface ISettingsService
    {
        Task<string?> GetApiKey(string provider);
        Task<string?> GetSystemPrompt(string claude);
        Task SaveApiKey(string provider, string apiKey);
        Task SaveSystemPrompt(string claude, string defaultSystemPrompt);
    }
}
