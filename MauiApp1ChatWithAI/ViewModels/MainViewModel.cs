using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Extensions;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly IChatDataManager _chatDataManager;
        private readonly ILLMApiService _llmService;
        private readonly ChatService _chatService;

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

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isThreadSelected;

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

            threadEventAggregator.ThreadSelected += OnThreadSelected;
            threadEventAggregator.ThreadCreated += OnThreadCreated;
            threadEventAggregator1 = threadEventAggregator;
            IsThreadSelected = false;
        }

        private async void OnThreadCreated(object? sender, string threadId)
        {
            // これを追加した。
            var thread = await _chatService.LoadThread(threadId);
            this.selectedThread = thread;
            IsThreadSelected = true;
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
                IsThreadSelected = true;
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

            IsLoading = true;
            try
            {
                var userMessageText = MessageInput;
                MessageInput = string.Empty;

                // ユーザーメッセージの表示
                var userMessage = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ThreadId = this.selectedThread.Id,
                    Role = "user",
                    Timestamp = DateTime.UtcNow,
                    MessageElements = new List<MessageElement>
                    {
                        new MessageElement { Id = Guid.NewGuid().ToString(), Type = "Text", Content = userMessageText, Timestamp = DateTime.UtcNow }
                    }
                };
                Messages.Add(CreateMessageDisplay(userMessage));

                // メッセージ送信と応答取得
                var messageId = await _chatService.SendMessage(
                    this.selectedThread.Id,
                    userMessageText
                );

                var responseMessages = _chatService.GetThreadMessages(this.selectedThread.Id);
                var responseMessage = responseMessages.LastOrDefault(m => m.Role == "assistant" && m.Id == messageId);

                if (responseMessage != null)
                {
                    Messages.Add(CreateMessageDisplay(responseMessage));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send message: {ex.Message}");
                await Shell.Current.DisplayAlert("エラー",
                    $"APIKeyが未登録です: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Message オブジェクトから MessageDisplay オブジェクトを作成する共通メソッド
        private MessageDisplay CreateMessageDisplay(Message message)
        {
            var messageDisplay = new MessageDisplay(message);
            messageDisplay.Parts = ParseMessage(message.MessageElements.FirstOrDefault()?.Content ?? "");
            return messageDisplay;
        }

        // メッセージを解析してテキストとコードスニペットに分割するメソッド
        private List<MessagePart> ParseMessage(string message)
        {
            var parts = new List<MessagePart>();
            var codeBlockRegex = new Regex("```(.*?)```", RegexOptions.Singleline);
            int currentIndex = 0;

            foreach (Match match in codeBlockRegex.Matches(message))
            {
                if (match.Index > currentIndex)
                {
                    parts.Add(new MessagePart { Type = "Text", Content = message.Substring(currentIndex, match.Index - currentIndex).Trim() });
                }
                parts.Add(new MessagePart { Type = "Code", Content = match.Groups[1].Value.Trim() });
                currentIndex = match.Index + match.Length;
            }

            if (currentIndex < message.Length)
            {
                parts.Add(new MessagePart { Type = "Text", Content = message.Substring(currentIndex).Trim() });
            }

            return parts;
        }

        [RelayCommand]
        private async Task OpenSettings()
        {
            await Shell.Current.GoToAsync("devmenu");
        }

        [RelayCommand]
        private async Task OpenThreadSettings()
        {
            await Shell.Current.GoToAsync(nameof(Views.ThreadCreatePage));
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

        public void Dispose()
        {
            threadEventAggregator1.ThreadSelected -= OnThreadSelected;
            threadEventAggregator1.ThreadCreated -= OnThreadCreated;
        }
    }
}