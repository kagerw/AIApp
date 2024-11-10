using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Service;
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
            ILLMApiService llmService)
        {
            _settingsService = settingsService;
            _llmService = llmService;
            Title = "開発者設定";

            // 初期タブ選択
            IsApiTabSelected = true;
        }

        [ObservableProperty]
        private string apiKey = string.Empty;

        [ObservableProperty]
        private string defaultSystemPrompt = string.Empty;

        [ObservableProperty]
        private bool isApiTabSelected;

        [ObservableProperty]
        private bool isPromptsTabSelected;

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
    }
}
