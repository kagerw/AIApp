using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public class SettingsService : ISettingsService
    {
        private readonly ISecureStorageService _secureStorage;
        private const string API_KEY_PREFIX = "api_key_";

        public SettingsService(ISecureStorageService secureStorage)
        {
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        }

        public async Task<string?> GetApiKey(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider cannot be empty", nameof(provider));

            try
            {
                return await _secureStorage.GetSecureValue($"{API_KEY_PREFIX}{provider.ToLower()}");
            }
            catch (Exception ex)
            {
                // ログ記録をここに追加することを推奨
                Debug.WriteLine($"Failed to get API key for provider {provider}: {ex.Message}");
                return null;
            }
        }

        public async Task SaveApiKey(string provider, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider cannot be empty", nameof(provider));

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty", nameof(apiKey));

            try
            {
                await _secureStorage.SetSecureValue($"{API_KEY_PREFIX}{provider.ToLower()}", apiKey);
            }
            catch (Exception ex)
            {
                // ログ記録をここに追加することを推奨
                Debug.WriteLine($"Failed to save API key for provider {provider}: {ex.Message}");
                throw new Exception($"Failed to save API key for provider {provider}", ex);
            }
        }

        public Task<string?> GetSystemPrompt(string claude)
        {
            return null;
        }

        public Task SaveSystemPrompt(string claude, string defaultSystemPrompt)
        {
            // まだ何もしない
            return Task.CompletedTask;
        }
    }
}