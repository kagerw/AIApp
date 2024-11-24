using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Service;
using MauiApp1ChatWithAI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class DevMenuViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILLMApiService _llmService;

        public DevMenuViewModel(
            ISettingsService settingsService,
            ILLMApiService llmService,
            IDatabaseService databaseService)
        {
            _settingsService = settingsService;
            _llmService = llmService;
            Title = "開発者設定";

            // 初期タブ選択
            IsApiTabSelected = true;

            this._databaseService = databaseService;
        }

        [ObservableProperty]
        private string apiKey = string.Empty;

        [ObservableProperty]
        private string defaultSystemPrompt = string.Empty;

        [ObservableProperty]
        private bool isApiTabSelected;

        [ObservableProperty]
        private bool isPromptsTabSelected;

        [ObservableProperty]
        private bool isDbTabSelected;

        public override async Task InitializeAsync()
        {
            // 保存済み設定の読み込み
            apiKey = await _settingsService.GetApiKey(AppConstants.Providers.Claude) ?? string.Empty;
            //defaultSystemPrompt = await _settingsService.GetSystemPrompt(AppConstants.Providers.Claude) ?? string.Empty;
        }

        [RelayCommand]
        private async Task ValidateApiKey()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                await Shell.Current.DisplayAlert("エラー", "APIキーを入力してください", "OK");
                return;
            }

            try
            {
                await _llmService.InitializeAsync(ApiKey);
                await Shell.Current.DisplayAlert("成功", "APIキーの検証に成功しました", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"APIキーの検証に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private void SwitchTab(string tab)
        {
            IsApiTabSelected = tab == "api";
            IsPromptsTabSelected = tab == "prompts";
            IsDbTabSelected = tab == "db";
        }

        [RelayCommand]
        private async Task Cancel() 
        {
            await Shell.Current.GoToAsync("..");  // 前の画面に戻る
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                await _settingsService.SaveApiKey(AppConstants.Providers.Claude, ApiKey);
                await _settingsService.SaveSystemPrompt(AppConstants.Providers.Claude, DefaultSystemPrompt);

                await Shell.Current.DisplayAlert("成功", "設定を保存しました", "OK");
                await Shell.Current.GoToAsync("..");  // 前の画面に戻る
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"設定の保存に失敗しました: {ex.Message}", "OK");
            }
        }
        
        [ObservableProperty]
        private bool isTesting;

        [ObservableProperty]
        private string statusMessage;

        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private string server;

        [ObservableProperty]
        private int port = 3306; // MySQLのデフォルトポート

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string databaseName;

        [RelayCommand]
        private async Task TestConnection()
        {
            if (IsTesting) return;

            try
            {
                IsTesting = true;
                StatusMessage = "接続テスト中...";

                var settings = new DatabaseSettings
                {
                    Server = Server,
                    Port = Port,
                    Username = Username,
                    Password = Password,
                    DatabaseName = DatabaseName
                };

                var success = await _databaseService.TestConnectionAsync(settings);
                StatusMessage = success ? "接続成功!" : "接続失敗";
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
            }
            finally
            {
                IsTesting = false;
            }
        }

        [RelayCommand]
        private async Task SaveDbSettings()
        {
            try
            {
                var settings = new DatabaseSettings
                {
                    Server = Server,
                    Port = Port,
                    Username = Username,
                    Password = Password,
                    DatabaseName = DatabaseName
                };

                await _databaseService.SaveSettingsAsync(settings);
                StatusMessage = "設定を保存しました";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存エラー: {ex.Message}";
            }
        }
    }
}
