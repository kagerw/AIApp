﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Extensions;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IChatDataManager _dataManager;
        private readonly ILLMApiService _llmService;
        private readonly ChatService _chatService;

        [ObservableProperty]
        private ObservableCollection<ChatThread> threads = new();

        [ObservableProperty]
        private string messageInput = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MessageDisplay> messages = new();

        [ObservableProperty]
        private ChatThread selectedThread;

        [ObservableProperty]
        private string newThreadTitle = string.Empty;

        [ObservableProperty]
        private bool isThreadListVisible;

        public MainViewModel(
            IChatDataManager dataManager,
            ILLMApiService llmService,
            ChatService chatService)
        {
            _dataManager = dataManager;
            _llmService = llmService;
            _chatService = chatService;
            Title = "Chat App";

            // 初期データ読み込み
            LoadThreadsAsync().FireAndForgetSafeAsync();
        }

        public override async Task InitializeAsync()
        {
            await LoadThreads();
        }

        [RelayCommand]
        private async Task LoadThreads()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var threads = await _dataManager.GetAllThreadsAsync();
                Threads.Clear();
                foreach (var thread in threads)
                {
                    Threads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの読み込みに失敗しました: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ToggleSidebar()
        {
            // Shell.FlyoutBehaviorの切り替え
            Shell.Current.FlyoutBehavior = Shell.Current.FlyoutBehavior
                == FlyoutBehavior.Flyout ? FlyoutBehavior.Disabled : FlyoutBehavior.Flyout;
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput) || SelectedThread == null)
                return;

            try
            {
                var content = MessageInput;
                MessageInput = string.Empty;  // 入力欄をクリア

                await _chatService.SendMessage(SelectedThread.Id, content);
                await LoadMessages(SelectedThread.Id);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"メッセージの送信に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            await Shell.Current.GoToAsync("devmenu");
        }

        private async Task LoadMessages(string threadId)
        {
            try
            {
                var messageList = await _dataManager.GetMessagesAsync(threadId);
                Messages.Clear();
                foreach (var message in messageList)
                {
                    Messages.Add(new MessageDisplay(message));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"メッセージの読み込みに失敗しました: {ex.Message}", "OK");
            }
        }

        partial void OnSelectedThreadChanged(ChatThread value)
        {
            if (value != null)
            {
                LoadMessages(value.Id).FireAndForgetSafeAsync();
            }
        }


        [RelayCommand]
        private async Task CreateThread()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var threadId = await _dataManager.CreateThreadAsync(
                    NewThreadTitle,
                    AppConstants.Providers.Claude
                );

                var thread = await _dataManager.GetThreadAsync(threadId);
                if (thread != null)
                {
                    Threads.Add(thread);
                    NewThreadTitle = string.Empty;  // 入力欄をクリア
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの作成に失敗しました: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadThreadsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var threadList = await _dataManager.GetAllThreadsAsync();
                Threads.Clear();
                foreach (var thread in threadList)
                {
                    Threads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの読み込みに失敗しました: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleThreadList()
        {
            IsThreadListVisible = !IsThreadListVisible;
        }
    }
}