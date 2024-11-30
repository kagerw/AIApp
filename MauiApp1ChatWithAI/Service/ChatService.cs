using MauiApp1ChatWithAI.Models.Database;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.Service
{
    public class ChatService
    {
        private readonly IChatDataManager _dataManager;
        private readonly ILLMApiService _llmService;
        private readonly Dictionary<string, List<Message>> _threadMessages = new();

        public ChatService(
            IChatDataManager dataManager,
            ILLMApiService llmService)  // コンストラクタインジェクション
        {
            _dataManager = dataManager;
            _llmService = llmService;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");
            Debug.WriteLine($"Database location: {dbPath}");
        }

        public async Task<ChatThread> LoadThread(string threadId)
        {
            if (!_threadMessages.ContainsKey(threadId))
            {
                var messages = await _dataManager.GetMessagesAsync(threadId);
                _threadMessages[threadId] = messages;
            }

            var thread = await _dataManager.GetThreadAsync(threadId);
            if (thread == null)
            {
                throw new ArgumentException($"Thread not found: {threadId}");
            }
            return thread;
        }
        public async Task<string> SendMessage(string threadId, string message)
        {
            var thread = await _dataManager.GetThreadAsync(threadId);
            if (thread == null)
            {
                throw new ArgumentException($"Thread not found: {threadId}");
            }

            // システムプロンプトの取得
            string? systemPrompt = thread.IsSystemPromptEnabled ? thread.SystemPrompt : null;

            // TODO : 何かうまい手はないか？
            await _llmService.LoadApiKey();

            // APIコール
            // memo:ここでエラーになった。なぜなら初期化をやってなかった。
            // memo：ここでエラーになった。なぜならAPI-Keyが未入力だったから。
            var response = await _llmService.GetResponseAsync(
                message,
                _threadMessages[threadId],
                threadId,
                systemPrompt
            );

            // DB保存
            var userMessageId = await _dataManager.AddMessageAsync(threadId, "user", message);
            var assistantMessageId = await _dataManager.AddMessageAsync(threadId, "assistant", response);

            // キャッシュ更新
            var userMessage = new Message
            {
                Id = userMessageId,
                ThreadId = threadId,
                Role = "user",
                Timestamp = DateTime.UtcNow,
                MessageElements = new List<MessageElement>
                {
                    new MessageElement
                    {
                        Id = Guid.NewGuid().ToString(),
                        MessageId = userMessageId,
                        Type = "Text",
                        Content = message,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            var assistantMessage = new Message
            {
                Id = assistantMessageId,
                ThreadId = threadId,
                Role = "assistant",
                Timestamp = DateTime.UtcNow,
                MessageElements = new List<MessageElement>
                {
                    new MessageElement
                    {
                        Id = Guid.NewGuid().ToString(),
                        MessageId = assistantMessageId,
                        Type = "Text",
                        Content = response,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            _threadMessages[threadId].Add(userMessage);
            _threadMessages[threadId].Add(assistantMessage);

            return assistantMessageId;
        }

        public List<Message> GetThreadMessages(string threadId)
        {
            return _threadMessages.GetValueOrDefault(threadId, new List<Message>());
        }
    }
}
