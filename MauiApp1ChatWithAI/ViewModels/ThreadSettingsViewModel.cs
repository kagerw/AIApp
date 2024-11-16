﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Service;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadSettingsViewModel : ViewModelBase
    {
        private readonly IChatDataManager _dataManager;
        private readonly ISettingsService _settingsService;
        private readonly IThreadEventAggregator _threadEventAggregator;

        public ThreadSettingsViewModel(
            IChatDataManager dataManager,
            ISettingsService settingsService,
            IThreadEventAggregator threadEventAggregator)
        {
            _dataManager = dataManager;
            _settingsService = settingsService;
            _threadEventAggregator = threadEventAggregator;

            Title = "スレッド設定"; // ViewModelBaseのTitleプロパティを設定

            // プロバイダー一覧の設定
            Providers = new List<string> { AppConstants.Providers.Claude };
            SelectedProvider = Providers[0];  // デフォルト選択
        }

        [ObservableProperty]
        private string threadTitle = string.Empty;

        [ObservableProperty]
        private List<string> providers;

        [ObservableProperty]
        private string selectedProvider;

        [ObservableProperty]
        private string systemPrompt = string.Empty;

        [ObservableProperty]
        private bool isSystemPromptEnabled;

        public override async Task InitializeAsync()
        {
            // デフォルトのシステムプロンプトを読み込み
            systemPrompt = await _settingsService.GetSystemPrompt(AppConstants.Providers.Claude) ?? string.Empty;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(ThreadTitle))
            {
                await Shell.Current.DisplayAlert("エラー", "スレッドタイトルを入力してください", "OK");
                return;
            }

            try
            {
                var threadId = await _dataManager.CreateThreadAsync(
                    ThreadTitle,
                    SelectedProvider,
                    IsSystemPromptEnabled ? SystemPrompt : null,
                    IsSystemPromptEnabled
                );

                // スレッドIDからスレッドオブジェクトを取得
                var thread = await _dataManager.GetThreadAsync(threadId);
                if (thread != null)
                {
                    _threadEventAggregator.PublishThreadCreated(thread);
                }

                await Shell.Current.GoToAsync("..");  // 前の画面に戻る
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの作成に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");  // 前の画面に戻る
        }
    }
}