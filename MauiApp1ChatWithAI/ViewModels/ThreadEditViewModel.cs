using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Service;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadEditViewModel : ViewModelBase
    {
        private readonly IChatDataManager _dataManager;
        private readonly ISettingsService _settingsService;
        private readonly IThreadEventAggregator _threadEventAggregator;
        private readonly string _threadId;

        public ThreadEditViewModel(
            IChatDataManager dataManager,
            ISettingsService settingsService,
            IThreadEventAggregator threadEventAggregator,
            string threadId)
        {
            _dataManager = dataManager;
            _settingsService = settingsService;
            _threadEventAggregator = threadEventAggregator;
            _threadId = threadId;

            Title = "スレッド編集";
            // プロバイダー一覧の設定
            Providers = new List<string> { AppConstants.Providers.Claude };
            SelectedProvider = Providers[0];
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

        [ObservableProperty]
        private DateTime createdAt;

        [ObservableProperty]
        private DateTime updatedAt;

        public override async Task InitializeAsync()
        {
            try
            {
                // スレッドデータの読み込み
                var thread = await _dataManager.GetThreadAsync(_threadId);
                if (thread != null)
                {
                    ThreadTitle = thread.Title;
                    SelectedProvider = thread.Provider;
                    SystemPrompt = thread.SystemPrompt ?? string.Empty;
                    IsSystemPromptEnabled = thread.IsSystemPromptEnabled;
                    CreatedAt = thread.CreatedAt;
                    UpdatedAt = thread.LastMessageAt;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの読み込みに失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Update()
        {
            if (string.IsNullOrWhiteSpace(ThreadTitle))
            {
                await Shell.Current.DisplayAlert("エラー", "スレッドタイトルを入力してください", "OK");
                return;
            }

            try
            {
                var updatedThread = await _dataManager.UpdateThreadTitleAsync(
                    _threadId,
                    ThreadTitle
                );

                if (updatedThread != null)
                {
                    _threadEventAggregator.PublishThreadUpdated(updatedThread);
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの更新に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}