using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadEditViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly IChatDataManager _dataManager;
        private readonly ISettingsService _settingsService;
        private readonly IThreadEventAggregator _threadEventAggregator;

        [ObservableProperty]
        private ChatThread currentThread;

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
        private bool isLoading;

        public ThreadEditViewModel(
            IChatDataManager dataManager,
            ISettingsService settingsService,
            IThreadEventAggregator threadEventAggregator)
        {
            _dataManager = dataManager;
            _settingsService = settingsService;
            _threadEventAggregator = threadEventAggregator;

            Title = "スレッド編集";
            Providers = new List<string> { AppConstants.Providers.Claude };
            SelectedProvider = Providers[0];
        }

        private async Task LoadCurrentThread()
        {
            if (IsBusy) return;
            IsBusy = true;
            IsLoading = true;

            try
            {
                if (CurrentThread?.Id != null)
                {
                    var thread = await _dataManager.GetThreadAsync(CurrentThread.Id);
                    if (thread != null)
                    {
                        CurrentThread = thread;
                        ThreadTitle = thread.Title;
                        SelectedProvider = thread.Provider;
                        SystemPrompt = thread.SystemPrompt ?? string.Empty;
                        IsSystemPromptEnabled = !string.IsNullOrEmpty(thread.SystemPrompt);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの読み込みに失敗しました: {ex.Message}", "OK");
                Debug.WriteLine($"Error loading thread: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
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
                if (CurrentThread?.Id != null)
                {
                    var updatedThread = await _dataManager.UpdateThreadTitleAsync(
                        CurrentThread.Id,
                        ThreadTitle);

                    if (updatedThread != null)
                    {
                        _threadEventAggregator.PublishThreadUpdated(updatedThread);
                    }

                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの更新に失敗しました: {ex.Message}", "OK");
                Debug.WriteLine($"Error updating thread: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task DeleteThread()
        {
            try
            {
                if (CurrentThread?.Id != null)
                {
                    // 削除前に確認ダイアログを表示
                    bool answer = await Shell.Current.DisplayAlert(
                        "確認",
                        "このスレッドを削除してもよろしいですか？",
                        "はい",
                        "いいえ"
                    );
                    if (answer)
                    {
                        await _dataManager.DeleteThreadAsync(currentThread.Id);
                        await Shell.Current.DisplayAlert("成功", "スレッドを削除しました。", "OK");
                        _threadEventAggregator.PublishThreadDeleeted(currentThread);
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                   $"スレッド削除に失敗しました: {ex.Message}", "OK");
                Debug.WriteLine($"Error copying to clipboard: {ex.Message}");
            }
        }

        // IQueryAttributable の実装
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Thread", out var threadObj) && threadObj is ChatThread thread)
            {
                CurrentThread = thread;
                await LoadCurrentThread();
            }
        }
    }
}
