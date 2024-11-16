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

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadListViewModel : ObservableObject
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
            threadEventAggregator1 = threadEventAggregator;
        }

        private void ThreadEventAggregator_ThreadCreated(object? sender, ChatThread thread)
        {
            Threads.Add(thread);
            selectedThread = thread;
        }

        [RelayCommand]
        private async Task LoadThreadsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                var threadList = await _chatDataManager.GetAllThreadsAsync();
                Threads = new ObservableCollection<ChatThread>(threadList);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedThreadChanged(ChatThread value)
        {
            sidebarTranslation = -300;
            threadEventAggregator1.PublishThreadSelected(value);
        }
    }
}
