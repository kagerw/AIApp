using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Extensions;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using MauiApp1ChatWithAI.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadListViewModel : ViewModelBase, IDisposable
    {
        private readonly IChatDataManager _chatDataManager;

        [ObservableProperty]
        private ObservableCollection<ChatThread> threads;

        [ObservableProperty]
        private ChatThread selectedThread;

        [ObservableProperty]
        private bool isLoading;

        private IThreadEventAggregator threadEventAggregator1;

        public ThreadListViewModel(IChatDataManager chatDataManager, IThreadEventAggregator threadEventAggregator)
        {
            _chatDataManager = chatDataManager;
            threads = new ObservableCollection<ChatThread>();
            LoadThreadsAsync().FireAndForgetSafeAsync();
            threadEventAggregator.ThreadCreated += ThreadEventAggregator_ThreadCreated;
            threadEventAggregator.ThreadsNeedReorder += ThreadEventAggregator_ThreadsNeedReorder;  // 追加
            threadEventAggregator.ThreadDeleted += ThreadEventAggregator_ThreadDeleted;
            threadEventAggregator.ThreadUpdated += ThreadEventAggregator_ThreadUpdated;
            threadEventAggregator1 = threadEventAggregator;
        }

        private void ThreadEventAggregator_ThreadUpdated(object? sender, ChatThread thread)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var index = Threads.ToList().FindIndex(t => t.Id == thread.Id);
                if (index != -1)
                {
                    Threads[index] = thread;  // 既存のアイテムを更新

                    // コレクションの変更を通知
                    OnPropertyChanged(nameof(Threads));
                }
            });
        }

        private void ThreadEventAggregator_ThreadDeleted(object? sender, ChatThread thread)
        {
            Debug.WriteLine($"ThreadEventAggregator_ThreadDeleted: {thread?.Title ?? "null"}, Id: {thread?.Id ?? "null"}");
            Threads.Remove(thread);
            Threads = new ObservableCollection<ChatThread>(
                Threads.OrderByDescending(t => t.LastMessageAt)
            );
            SelectedThread = null;
        }

        private async void ThreadEventAggregator_ThreadCreated(object? sender, string threadId)
        {
            try
            {
                // 新しく作成されたスレッドを取得
                var newThread = await _chatDataManager.GetThreadAsync(threadId);
                if (newThread == null)
                {
                    Debug.WriteLine($"Thread with ID {threadId} not found");
                    return;
                }

                // すべてのスレッドを取得して最新順に並べ替え
                var allThreads = await _chatDataManager.GetAllThreadsAsync();

                // ObservableCollectionを更新
                Threads = new ObservableCollection<ChatThread>(
                    allThreads.OrderByDescending(t => t.LastMessageAt)
                );

                // 選択中のスレッドを新しく作成されたものに設定
                SelectedThread = newThread;

                Debug.WriteLine($"Thread added successfully: {newThread.Title}, Id: {threadId}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ThreadEventAggregator_ThreadCreated: {ex.Message}");
                // 必要に応じてエラーハンドリングを追加
            }
        }

        [RelayCommand]
        private async Task LoadThreadsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                var threadList = await _chatDataManager.GetAllThreadsAsync();
                Threads = new ObservableCollection<ChatThread>(
                    threadList.OrderByDescending(t => t.LastMessageAt)
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task GoToThreadDetails(ChatThread thread)
        {
            if (thread != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "Thread", thread }
                };
                await Shell.Current.GoToAsync(nameof(ThreadEditPage), parameters);
            }
        }

        partial void OnSelectedThreadChanged(ChatThread value)
        {
            Debug.WriteLine($"Selected thread changed: {value?.Title ?? "null"}, Id: {value?.Id ?? "null"}");
            if (value != null)
            {
                Debug.WriteLine($"IsSelected? Threads contains thread: {Threads.Contains(value)}");
                Debug.WriteLine($"Current threads count: {Threads.Count}");
            }
            if (value != null)
            {
                threadEventAggregator1.PublishThreadSelected(value);
            }
        }

        private void ThreadEventAggregator_ThreadsNeedReorder(object? sender, EventArgs e)
        {
            Threads = new ObservableCollection<ChatThread>(
                Threads.OrderByDescending(t => t.LastMessageAt)
            );
        }

        public void Dispose()
        {
            threadEventAggregator1.ThreadCreated -= ThreadEventAggregator_ThreadCreated;
            threadEventAggregator1.ThreadsNeedReorder -= ThreadEventAggregator_ThreadsNeedReorder;  // 追加
        }
    }
}
