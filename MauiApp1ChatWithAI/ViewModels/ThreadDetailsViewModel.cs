using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadDetailsViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly IChatDataManager _chatDataManager;
        private readonly IThreadEventAggregator _threadEventAggregator;

        [ObservableProperty]
        private ChatThread currentThread;

        [ObservableProperty]
        private bool hasSystemPrompt;

        [ObservableProperty]
        private bool isLoading;

        private IChatDataManager _dataManager;

        public ThreadDetailsViewModel(
            IChatDataManager chatDataManager,
            IThreadEventAggregator threadEventAggregator,
            IChatDataManager dataManager)
        {
            _chatDataManager = chatDataManager;
            _threadEventAggregator = threadEventAggregator;
            _dataManager = dataManager;
        }

        public override async Task InitializeAsync()
        {
            await LoadCurrentThread();
        }

        private async Task LoadCurrentThread()
        {
            if (IsBusy) return;
            IsBusy = true;
            IsLoading = true;

            try
            {
                // もし必要なら、前の画面から渡されたスレッドIDを使用
                if (CurrentThread?.Id != null)
                {
                    CurrentThread = await _chatDataManager.GetThreadAsync(CurrentThread.Id);
                    HasSystemPrompt = !string.IsNullOrEmpty(CurrentThread?.SystemPrompt);
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
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CopySystemPrompt()
        {
            if (string.IsNullOrEmpty(CurrentThread?.SystemPrompt))
                return;

            try
            {
                await Clipboard.SetTextAsync(CurrentThread.SystemPrompt);
                await Shell.Current.DisplayAlert("成功", "システムプロンプトをクリップボードにコピーしました", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"クリップボードへのコピーに失敗しました: {ex.Message}", "OK");
                Debug.WriteLine($"Error copying to clipboard: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleeteThread()
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
