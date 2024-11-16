using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithClaude.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public class ChatGPTService : ILLMApiService  // 既存のIClaudeServiceを実装
    {
        private readonly HttpClient _client;
        private readonly ISecureStorageService _secureStorage;
        private readonly List<Message> _conversationHistory;
        private string _apiKey;

        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        private const string API_KEY_STORAGE_KEY = "chatgpt_api_key";
        private const int MAX_HISTORY_LENGTH = 10;

        public bool IsInitialized => !string.IsNullOrEmpty(_apiKey);
        public IReadOnlyList<Message> ConversationHistory => _conversationHistory.AsReadOnly();

        public ChatGPTService(ISecureStorageService secureStorage)
        {
            _client = new HttpClient();
            _secureStorage = secureStorage;
            _conversationHistory = new List<Message>();
        }

        public async Task<bool> LoadApiKey()
        {
            try
            {
                _apiKey = await _secureStorage.GetSecureValue(API_KEY_STORAGE_KEY);
                return IsInitialized;
            }
            catch (Exception)
            {
                _apiKey = null;
                return false;
            }
        }

        public async Task ClearApiKey()
        {
            try
            {
                await _secureStorage.RemoveSecureValue(API_KEY_STORAGE_KEY);
                _apiKey = null;
                ClearHistory();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clear API key", ex);
            }
        }

        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }

        public async Task InitializeAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty", nameof(apiKey));

            try
            {
                // APIキーの有効性を確認するための簡単なテストメッセージ
                var testMessage = new
                {
                    model = "gpt-4",
                    messages = new[] { new { role = "user", content = "test" } }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, OPENAI_API_URL)
                {
                    Content = new StringContent(JsonSerializer.Serialize(testMessage), Encoding.UTF8, "application/json")
                };

                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // APIキーが有効な場合のみ保存
                await _secureStorage.SetSecureValue(API_KEY_STORAGE_KEY, apiKey);
                _apiKey = apiKey;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize with provided API key. Please check if the key is valid.", ex);
            }
        }

        public async Task<string> GetResponseAsync(string message)
        {
            return null;
            //if (!IsInitialized)
            //    throw new InvalidOperationException("Service is not initialized. Please set API key first.");

            //// 新しいメッセージを履歴に追加
            //_conversationHistory.Add(new ChatMessage("user", message));

            //// システムプロンプトを取得
            //var systemPrompt = await _secureStorage.GetSecureValue($"system_prompt_{LLMProvider.ChatGPT}");

            //var messages = new List<object>();

            //// システムプロンプトがある場合は追加
            //if (!string.IsNullOrEmpty(systemPrompt))
            //{
            //    messages.Add(new { role = "system", content = systemPrompt });
            //}

            //// 会話履歴を追加
            //messages.AddRange(_conversationHistory
            //    .TakeLast(MAX_HISTORY_LENGTH)
            //    .Select(m => new { role = m.Role.ToLower(), content = m.Content }));

            //var requestData = new
            //{
            //    model = "gpt-4",
            //    messages = messages.ToArray(),
            //    max_tokens = 1024,
            //    temperature = 0.7
            //};

            //string jsonContent = JsonSerializer.Serialize(requestData);

            //var request = new HttpRequestMessage(HttpMethod.Post, OPENAI_API_URL)
            //{
            //    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            //};

            //request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            //var response = await _client.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            //string responseBody = await response.Content.ReadAsStringAsync();

            //using (JsonDocument document = JsonDocument.Parse(responseBody))
            //{
            //    var responseText = document.RootElement
            //        .GetProperty("choices")[0]
            //        .GetProperty("message")
            //        .GetProperty("content")
            //        .GetString();

            //    // アシスタントの応答を履歴に追加
            //    _conversationHistory.Add(new ChatMessage("assistant", responseText));

            //    return responseText;
            //}
        }
    }
}
