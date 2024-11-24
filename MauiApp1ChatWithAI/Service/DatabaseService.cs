using MauiApp1ChatWithAI.Service;
using MauiApp1ChatWithAI.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ISecureStorageService _secureStorage;
        private const string SETTINGS_KEY = "mysql_settings";

        public DatabaseService(ISecureStorageService secureStorage)
        {
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
        }

        public async Task<bool> TestConnectionAsync(DatabaseSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            try
            {
                using var connection = new MySqlConnection(settings.ConnectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MySQL接続テストエラー: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"予期せぬエラー: {ex.Message}");
                return false;
            }
        }

        public async Task SaveSettingsAsync(DatabaseSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            try
            {
                var json = JsonSerializer.Serialize(settings);
                await _secureStorage.SetSecureValue(SETTINGS_KEY, json);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("データベース設定の保存に失敗しました", ex);
            }
        }

        public async Task<DatabaseSettings> LoadSettingsAsync()
        {
            try
            {
                var json = await _secureStorage.GetSecureValue(SETTINGS_KEY);
                if (string.IsNullOrEmpty(json))
                {
                    return new DatabaseSettings
                    {
                        Port = 3306,
                        Server = "localhost"
                    };
                }

                return JsonSerializer.Deserialize<DatabaseSettings>(json)
                       ?? new DatabaseSettings { Port = 3306, Server = "localhost" };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("データベース設定の読み込みに失敗しました", ex);
            }
        }
    }
}
