using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Extensions;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IChatDataManager _chatDataManager;
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

        [ObservableProperty]
        private double sidebarTranslation = -300;

        [ObservableProperty]
        private bool isSidebarOpen;

        private IThreadEventAggregator threadEventAggregator1;

        public MainViewModel(
            IChatDataManager dataManager,
            ILLMApiService llmService,
            ChatService chatService,
            IThreadEventAggregator threadEventAggregator)
        {
            _chatDataManager = dataManager;
            _llmService = llmService;
            _chatService = chatService;
            Title = "Chat App";
            // 初期データ読み込み
            LoadThreadsAsync().FireAndForgetSafeAsync();

            threadEventAggregator.ThreadSelected += OnThreadSelected;
            threadEventAggregator.ThreadCreated += OnThreadCreated;
            threadEventAggregator1 = threadEventAggregator;
        }

        private async void OnThreadCreated(object? sender, ChatThread thread)
        {
            // これを追加した。
            await _chatService.LoadThread(thread.Id);
            this.selectedThread = thread;
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
                var threads = await _chatDataManager.GetAllThreadsAsync();
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
        private async Task OpenDevMenu()
        {
            await Shell.Current.GoToAsync(nameof(Views.DevMenuPage));
        }

        private async void OnThreadSelected(object sender, ChatThread thread)
        {
            // スレッド選択時にサイドバーを閉じる
            IsSidebarOpen = false;
            SidebarTranslation = -300;

            // 会話履歴の取得
            try
            {
                // これを追加した。
                await _chatService.LoadThread(thread.Id);
                var messageHistory = await _chatDataManager.GetMessagesAsync(thread.Id);
                Messages = new ObservableCollection<MessageDisplay>(
                    messageHistory.Select(m => new MessageDisplay(m))
                );

                this.selectedThread = thread;
                IsSidebarOpen = false;
                SidebarTranslation = IsSidebarOpen ? 0 : -300;

                Debug.WriteLine($"Loaded messages count: {messageHistory.Count}");
            }
            catch (Exception ex)
            {
                // エラーハンドリング
                Debug.WriteLine($"Failed to load messages: {ex.Message}");
                Messages.Clear();
                await Shell.Current.DisplayAlert("エラー",
                    $"スレッドの変更に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(MessageInput))
                return;

            try
            {
                var userMessage = MessageInput;
                MessageInput = string.Empty;  // 先にクリア

                // ユーザーメッセージの表示
                var userMessageObject = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ThreadId = this.selectedThread.Id,
                    Role = "user",
                    Timestamp = DateTime.UtcNow,
                    MessageElements = new List<MessageElement>
                    {
                        new MessageElement
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = "Text",
                            Content = userMessage,
                            Timestamp = DateTime.UtcNow
                        }
                    }
                };
                Messages.Add(new MessageDisplay(userMessageObject));

                // メッセージ送信と応答取得
                // メモ：ここでエラーになった、なぜなら初期化をやってなかった。
                var messageId = await _chatService.SendMessage(
                    this.selectedThread.Id,
                    userMessage
                );

                var messages = _chatService.GetThreadMessages(this.selectedThread.Id);
                var responseMessage = messages.LastOrDefault();

                if (responseMessage != null)
                {
                    Messages.Add(new MessageDisplay(responseMessage));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send message: {ex.Message}");
                // TODO: エラー表示
                await Shell.Current.DisplayAlert("エラー",
                    $"APIKeyが未登録です: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            await Shell.Current.GoToAsync("devmenu");
        }

        [RelayCommand]
        private async Task OpenThreadSettings()
        {
            await Shell.Current.GoToAsync(nameof(Views.ThreadSettingsPage));
        }

        [RelayCommand]
        private void ToggleThreadList()
        {
            IsSidebarOpen = !IsSidebarOpen;
            SidebarTranslation = IsSidebarOpen ? 0 : -300;
            Debug.WriteLine($"Toggling sidebar: IsSidebarOpen={IsSidebarOpen}, SidebarTranslation={SidebarTranslation}");

            if (IsSidebarOpen)
            {
                threadEventAggregator1.PublishThreadsNeedReorder();
            }
        }

        [RelayCommand]
        private void CloseSidebar()
        {
            IsSidebarOpen = false;
            // SidebarTranslationを更新するコードを追加
            SidebarTranslation = IsSidebarOpen ? 0 : -300;
            Debug.WriteLine($"Toggling sidebar: IsSidebarOpen={IsSidebarOpen}, SidebarTranslation={SidebarTranslation}");
        }

        private async Task LoadMessages(string threadId)
        {
            try
            {
                var messageList = await _chatDataManager.GetMessagesAsync(threadId);
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
                var threadId = await _chatDataManager.CreateThreadAsync(
                    NewThreadTitle,
                    AppConstants.Providers.Claude
                );

                var thread = await _chatDataManager.GetThreadAsync(threadId);
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
                var threadList = await _chatDataManager.GetAllThreadsAsync();
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

        public void Dispose()
        {
        }
    }
}