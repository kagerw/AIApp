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
        private readonly ISettingsService _settingsService;
        private const string PROVIDER_NAME = "claude";
        private const string ANTHROPIC_API_URL = "https://api.anthropic.com/v1/messages";

        public bool IsInitialized => !string.IsNullOrEmpty(_apiKey);

        // 既存のコンストラクタを維持
        public ClaudeApiService(ISettingsService settingsService)
        {
            _httpClient = new HttpClient();
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        }

        // InitializeAsync は既存のまま維持
        public async Task InitializeAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty", nameof(apiKey));

            try
            {
                // APIキーの検証ロジック（既存のまま）
                var request = new HttpRequestMessage(HttpMethod.Post, "messages");
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

                request.Content = JsonContent.Create(testContent);
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // APIキーが有効な場合のみ保存
                await _settingsService.SaveApiKey(PROVIDER_NAME, apiKey);
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

        public async Task<bool> LoadApiKey()
        {
            try
            {
                _apiKey = await _settingsService.GetApiKey(PROVIDER_NAME);
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
                await _settingsService.SaveApiKey(PROVIDER_NAME, string.Empty);
                _apiKey = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to clear API key", ex);
            }
        }

        public async Task<string> GetResponseAsync(string message, List<Message> conversationHistory, string threadId, string? systemPrompt = null)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("API key not initialized");

            try
            {
                // 会話履歴をAPI仕様に変換
                var apiMessages = conversationHistory.Select(m => new
                {
                    role = m.Role,
                    content = string.Join("\n", m.MessageElements.Select(e => e.Content))
                }).ToList();

                // 新しいユーザーメッセージを履歴に追加
                apiMessages.Add(new
                {
                    role = "user",
                    content = message
                });

                // リクエストデータを作成（SystemPromptをトップレベルに追加）
                var testMessage = new
                {
                    model = "claude-3-5-sonnet-20241022",
                    max_tokens = 4096,
                    system = systemPrompt ?? "", // SystemPromptをトップレベルに設定
                    messages = apiMessages      // メッセージ履歴
                };

                // JSONにシリアライズ
                string jsonContent = JsonSerializer.Serialize(testMessage, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                Debug.WriteLine($"Request JSON with SystemPrompt at top-level:\n{jsonContent}");

                // HTTPリクエストを作成
                var request = new HttpRequestMessage(HttpMethod.Post, ANTHROPIC_API_URL)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                // ヘッダーを追加
                request.Headers.Add("x-api-key", _apiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");

                // リクエストを送信
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error Response: {errorBody}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {errorBody}");
                }

                // レスポンスを解析
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response Content:\n{responseBody}");

                using (JsonDocument document = JsonDocument.Parse(responseBody))
                {
                    var responseText = document.RootElement
                        .GetProperty("content")
                        .EnumerateArray()
                        .First()
                        .GetProperty("text")
                        .GetString();

                    return responseText;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception details: {ex}");
                throw;
            }
        }


        private class ClaudeResponse
        {
            public string Content { get; set; } = string.Empty;
        }
    }
}
