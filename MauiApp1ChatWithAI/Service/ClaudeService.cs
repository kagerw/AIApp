using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithClaude.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public class ClaudeService : ILLMApiService
    {
        private readonly HttpClient _client;
        private readonly ISecureStorageService _secureStorage;
        private readonly List<Message> _conversationHistory;
        private string _apiKey;

        private const string ANTHROPIC_API_URL = "https://api.anthropic.com/v1/messages";
        private const string API_KEY_STORAGE_KEY = "claude_api_key";
        private const int MAX_HISTORY_LENGTH = 10;

        public bool IsInitialized => !string.IsNullOrEmpty(_apiKey);
        public IReadOnlyList<Message> ConversationHistory => _conversationHistory.AsReadOnly();

        public ClaudeService(ISecureStorageService secureStorage)
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
                    model = "claude-3-5-sonnet-20241022",
                    max_tokens = 10,
                    messages = new[] { new { role = "user", content = "test" } }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, ANTHROPIC_API_URL)
                {
                    Content = new StringContent(JsonSerializer.Serialize(testMessage), Encoding.UTF8, "application/json")
                };

                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");

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
            if (!IsInitialized)
                throw new InvalidOperationException("Service is not initialized. Please set API key first.");

            try
            {
                // 新しいメッセージを履歴に追加
                //_conversationHistory.Add(new Message("user", message));

                // システムプロンプトを取得
                var systemPrompt = await _secureStorage.GetSecureValue($"system_prompt_{LLMProvider.Claude}");

                // Claudeの新しい仕様に合わせてリクエストを構築
                var requestData = new
                {
                    model = "claude-3-sonnet-20240229",
                    max_tokens = 1024,
                    system = systemPrompt ?? "",  // システムプロンプトはtop-levelパラメータとして渡す
                    
                    // TODO:
                    //messages = _conversationHistory
                    //    .TakeLast(MAX_HISTORY_LENGTH)
                    //    .Select(m => new { role = m.Role.ToLower(), content = m.Content })
                    //    .ToArray()
                };

                string jsonContent = JsonSerializer.Serialize(requestData);
                Debug.WriteLine($"Request JSON: {jsonContent}");

                var request = new HttpRequestMessage(HttpMethod.Post, ANTHROPIC_API_URL)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("x-api-key", _apiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");

                var response = await _client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error Response: {errorBody}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {errorBody}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();

                using (JsonDocument document = JsonDocument.Parse(responseBody))
                {
                    var responseText = document.RootElement
                        .GetProperty("content")
                        .EnumerateArray()
                        .First()
                        .GetProperty("text")
                        .GetString();

                    // TODO:
                    //_conversationHistory.Add(new Message("assistant", responseText));

                    return responseText;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception details: {ex}");
                throw;
            }
        }
    }

}
