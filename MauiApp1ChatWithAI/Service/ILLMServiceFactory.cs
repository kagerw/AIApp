using MauiApp1ChatWithClaude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public interface ILLMServiceFactory
    {
        ILLMApiService CreateService(LLMProvider provider);  // 指定されたAIを用意する
        LLMProvider CurrentProvider { get; }                 // 今どっちのAIを使ってるか
        void SetProvider(LLMProvider provider);             // AIを切り替える
    }
}
