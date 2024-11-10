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

        public event EventHandler<ChatThread> ThreadSelected;

        public ThreadListViewModel(IChatDataManager chatDataManager)
        
        
        {
            _chatDataManager = chatDataManager;
            threads = new ObservableCollection<ChatThread>();
            LoadThreadsAsync().FireAndForgetSafeAsync();
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

        [RelayCommand]
        private void SelectThread(ChatThread thread)
        {
            SelectedThread = thread;
            ThreadSelected?.Invoke(this, thread);
        }

        partial void OnSelectedThreadChanged(ChatThread value)
        {
            ThreadSelected?.Invoke(this, value);
        }
    }
}
