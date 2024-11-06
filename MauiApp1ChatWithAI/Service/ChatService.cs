using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public class ChatService
    {

        //private readonly IChatDataManager _dataManager;
        //private readonly IClaudeService _claudeService;
        //private readonly Dictionary<string, List<Message>> _threadMessages = new();
        //private IClaudeService _currentService; // 現在のスレッド用のサービス

        //// スレッドの履歴管理
        //public async Task LoadThread(string threadId)
        //{
        //    if (!_threadMessages.ContainsKey(threadId))
        //    {
        //        var messages = await _dataManager.GetMessagesAsync(threadId);
        //        _threadMessages[threadId] = messages;
        //    }

        //    // スレッドに対応したサービスを作成
        //    var thread = await _dataManager.GetThreadAsync(threadId);
        //    _currentService = _llmFactory.CreateService(thread.Provider);
        //}

        //public async Task<string> SendMessage(string threadId, string message)
        //{
        //    // キャッシュされた履歴を使ってAPIコール
        //    var response = await _claudeService.GetResponseAsync(
        //        message,
        //        _threadMessages[threadId]
        //    );

        //    // DB保存と履歴更新
        //    await _dataManager.AddMessageAsync(threadId, "user", message);
        //    var messageId = await _dataManager.AddMessageAsync(threadId, "assistant", response);

        //    // キャッシュ更新
        //    _threadMessages[threadId].Add(/* new message */);
        //    _threadMessages[threadId].Add(/* response message */);

        //    return messageId;
        //}

        //// 履歴取得
        //public List<Message> GetThreadMessages(string threadId)
        //{
        //    return _threadMessages.GetValueOrDefault(threadId, new List<Message>());
        //}

    }
}
