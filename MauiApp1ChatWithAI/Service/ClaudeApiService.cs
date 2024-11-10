using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithClaude.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public class ClaudeApiService : ILLMApiService
    {
        private readonly HttpClient _httpClient;
        private string? _apiKey;
        private readonly ISecureStorageService _secureStorage;
        private const string API_KEY_STORAGE_KEY = "claude_api_key";
        private const string ANTHROPIC_API_URL = "https://api.anthropic.com/v1/messages";

        public bool IsInitialized => !string.IsNullOrEmpty(_apiKey);

        // 既存のコンストラクタを維持
        public ClaudeApiService(ISecureStorageService secureStorage)
        {
            _httpClient = new HttpClient(); ;
            _secureStorage = secureStorage;
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        }

        // InitializeAsync は既存のまま維持
        public async Task InitializeAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty", nameof(apiKey));

            try
            {
                // リクエストの作成
                var request = new HttpRequestMessage(HttpMethod.Post, "messages");

                // ヘッダーの設定
                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");

                var testContent = new
                {
                    model = "claude-3-sonnet-20240229",
                    messages = new[]
                    {
                        new { role = "user", content = "test" }
                    },
                    max_tokens = 10
                };

                request.Content = JsonContent.Create(testContent, options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // APIキーが有効な場合のみ保存
                await _secureStorage.SetSecureValue(API_KEY_STORAGE_KEY, apiKey);
                _apiKey = apiKey;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("無効なAPIキーです。正しいAPIキーを入力してください。", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("APIキーの初期化に失敗しました。", ex);
            }
        }

        // LoadApiKeyとClearApiKeyを修正
        public async Task<bool> LoadApiKey()
        {
            try
            {
                _apiKey = await _secureStorage.GetSecureValue(API_KEY_STORAGE_KEY);
                return IsInitialized;
            }
            catch
            {
                return false;
            }
        }

        public async Task ClearApiKey()
        {
            try
            {
                await _secureStorage.RemoveSecureValue(API_KEY_STORAGE_KEY);
                _apiKey = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clear API key", ex);
            }
        }

        // GetResponseAsync は既存のまま維持
        public async Task<string> GetResponseAsync(string message, List<Message> conversationHistory, string? systemPrompt = null)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("API key not initialized");

            var messages = new List<object>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new { role = "system", content = systemPrompt });
            }

            foreach (var msg in conversationHistory)
            {
                var content = string.Join("\n", msg.MessageElements.Select(e => e.Content));
                messages.Add(new { role = msg.Role, content = content });
            }

            messages.Add(new { role = "user", content = message });
            
            var testMessage = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 4096,
                messages = new[] { new { role = "user", content = message } }
            };

            // ヘッダーの設定
            var request = new HttpRequestMessage(HttpMethod.Post, ANTHROPIC_API_URL)
            {
                Content = new StringContent(JsonSerializer.Serialize(testMessage), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Error Response: {errorBody}");
                throw new HttpRequestException($"API Error: {response.StatusCode} - {errorBody}");
            }
            string responseBody = await response.Content.ReadAsStringAsync();

            using (JsonDocument document = JsonDocument.Parse(responseBody))
            {
                try
                {
                    var responseText = document.RootElement
                    .GetProperty("content")
                    .EnumerateArray()
                    .First()
                    .GetProperty("text")
                    .GetString();
                    return responseText ?? throw new Exception("Empty response from API"); ;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON Parse Error: {ex.Message}");
                    throw new Exception($"Failed to parse API response: {ex.Message}", ex);
                }
            }
        }

        private class ClaudeResponse
        {
            public string Content { get; set; } = string.Empty;
        }
    }
}
