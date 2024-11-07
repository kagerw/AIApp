using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Extensions;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using System.Collections.ObjectModel;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class TestViewModel : ObservableObject
    {
        private readonly ILLMApiService _llmService;
        private readonly IChatDataManager _chatDataManager;
        private readonly ChatService _chatService;
        private string _threadId = string.Empty;
        private bool _isInitialized;

        [ObservableProperty]
        private string apiKey = string.Empty;

        [ObservableProperty]
        private string messageInput = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MessageDisplay> messages = new();

        [ObservableProperty]
        private bool isApiKeyVisible = true;  // APIキー入力欄の表示制御用

        public TestViewModel(
            ILLMApiService llmService,
            IChatDataManager chatDataManager,
            ChatService chatService)
        {
            _llmService = llmService;
            _chatDataManager = chatDataManager;
            _chatService = chatService;

            // 初期化処理を開始
            InitializeAsync().FireAndForgetSafeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // 保存済みのAPIキーを読み込み
                _isInitialized = await _llmService.LoadApiKey();

                if (_isInitialized)
                {
                    IsApiKeyVisible = false;  // APIキー入力欄を非表示
                    await CreateTestThread();  // テストスレッドの作成
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"初期化中にエラーが発生しました: {ex.Message}", "OK");
            }
        }

        private async Task CreateTestThread()
        {
            try
            {
                _threadId = await _chatDataManager.CreateThreadAsync(
                    $"Test Thread {DateTime.Now:yyyy/MM/dd HH:mm:ss}",
                    "Claude"
                );
                await _chatService.LoadThread(_threadId);

                Messages.Add(new MessageDisplay(new Message
                {
                    Role = "system",
                    MessageElements = new List<MessageElement>
                {
                    new MessageElement
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = "Text",
                        Content = "テストスレッドが作成されました。メッセージを入力してください。",
                        Timestamp = DateTime.UtcNow
                    }
                },
                    Timestamp = DateTime.UtcNow
                }));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"テストスレッドの作成に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task Initialize()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                await Shell.Current.DisplayAlert("エラー", "APIキーを入力してください", "OK");
                return;
            }

            try
            {
                await _llmService.InitializeAsync(ApiKey);
                _isInitialized = true;
                IsApiKeyVisible = false;

                await CreateTestThread();
                await Shell.Current.DisplayAlert("成功", "初期化が完了しました", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー",
                    $"初期化に失敗しました: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (!_isInitialized)
            {
                await Shell.Current.DisplayAlert("エラー", "APIキーを設定してください。", "OK");
                return;
            }

            try
            {
                var userMessage = MessageInput;
                var userMessageObject = new Message
                {
                    Id = Guid.NewGuid().ToString(),  // 一時的なID
                    ThreadId = _threadId,
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
                MessageInput = string.Empty;

                var messageId = await _chatService.SendMessage(_threadId, userMessage);
                var messages = _chatService.GetThreadMessages(_threadId);
                var responseMessage = messages.LastOrDefault();

                if (responseMessage != null)
                {
                    Messages.Add(new MessageDisplay(responseMessage));
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("エラー", $"メッセージの送信に失敗しました: {ex.Message}", "OK");
            }
        }
        private bool CanSendMessage() => !string.IsNullOrEmpty(MessageInput);
    }

    // 表示用のラッパークラス
    public class MessageDisplay : ObservableObject
    {
        private readonly Message _message;

        public MessageDisplay(Message message)
        {
            _message = message;
        }

        public string Role => _message.Role;

        public string Content => string.Join("\n",
            _message.MessageElements
                .Select(e => FormatContent(e)));

        private string FormatContent(MessageElement element)
        {
            return element.Type switch
            {
                "Code" => $"```{element.Language}\n{element.Content}\n```",
                "Text" => element.Content,
                _ => element.Content
            };
        }
    }
}