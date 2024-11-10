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
        Task SaveApiKey(string provider, string apiKey);
        Task<string?> GetSystemPrompt(string provider);
        Task SaveSystemPrompt(string provider, string prompt);
    }
}
