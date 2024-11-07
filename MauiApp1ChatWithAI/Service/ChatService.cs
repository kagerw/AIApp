using MauiApp1ChatWithAI.Models.Database;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.Service
{
    public class ChatService
    {
        private readonly IChatDataManager _dataManager;
        private readonly ILLMApiService _llmService;
        private readonly Dictionary<string, List<Message>> _threadMessages = new();
        private readonly Dictionary<string, ILLMApiService> _serviceCache = new();
        private readonly IServiceProvider _serviceProvider;

        public ChatService(
            IChatDataManager dataManager,
            ILLMApiService llmService)  // コンストラクタインジェクション
        {
            _dataManager = dataManager;
            _llmService = llmService;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");
            Debug.WriteLine($"Database location: {dbPath}");
        }


        private ILLMApiService GetServiceForThread(string provider)
        {
            if (!_serviceCache.ContainsKey(provider))
            {
                // ServiceProviderからサービスを取得
                _serviceCache[provider] = provider.ToLower() switch
                {
                    "claude" => _serviceProvider.GetRequiredService<ClaudeApiService>(),
                    // 他のプロバイダーを追加する場合はここに
                    _ => throw new ArgumentException($"Unknown provider: {provider}")
                };
            }
            return _serviceCache[provider];
        }

        public async Task LoadThread(string threadId)
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

            // APIコール
            var response = await _llmService.GetResponseAsync(
                message,
                _threadMessages[threadId],
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
