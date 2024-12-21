// Models/MessageDisplay.cs
using MauiApp1ChatWithAI.Models.Database;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel; // Clipboard を使用するため

namespace MauiApp1ChatWithAI.Models
{
    public class MessageDisplay
    {
        public string Role { get; set; }
        public List<MessagePart> Parts { get; set; } // メッセージのパーツをリストで保持
        public DateTime Timestamp { get; set; }

        public MessageDisplay(Message message)
        {
            Role = message.Role;
            Timestamp = message.Timestamp;
            Parts = new List<MessagePart>();
            foreach (var element in message.MessageElements)
            {
                Parts.Add(new MessagePart
                {
                    Type = element.Type,
                    Content = element.Content
                });
            }
        }
    }

    public partial class MessagePart : ObservableObject // ObservableObject から継承
    {
        [ObservableProperty]
        private string type;

        [ObservableProperty]
        private string content;

        [RelayCommand]
        private async Task CopyCode()
        {
            if (Type == "Code")
            {
                await Clipboard.SetTextAsync(Content);
                // コピー完了のフィードバックを表示する（オプション）
                await Shell.Current.DisplayAlert("コピー完了", "コードをクリップボードにコピーしました。", "OK");
            }
        }
    }
}