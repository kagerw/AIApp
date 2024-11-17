using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models.Database;
using System.Collections.ObjectModel;
using MauiApp1ChatWithAI.Service;
using MauiApp1ChatWithAI.Extensions;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadListViewModel : ObservableObject, IDisposable
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
            threadEventAggregator1 = threadEventAggregator;
        }

        private void ThreadEventAggregator_ThreadCreated(object? sender, ChatThread thread)
        {
            Debug.WriteLine($"ThreadEventAggregator_ThreadCreated: {thread?.Title ?? "null"}, Id: {thread?.Id ?? "null"}");
            Threads = new ObservableCollection<ChatThread>(
                Threads.Concat(new[] { thread })
                .OrderByDescending(t => t.LastMessageAt)
            );
            SelectedThread = thread;
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
