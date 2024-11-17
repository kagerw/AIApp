using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.ViewModels
{
    public partial class ThreadDetailsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ChatThread currentThread;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(nameof(CurrentThread), out var thread))
            {
                CurrentThread = thread as ChatThread;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                // エラーハンドリング
                Debug.WriteLine($"Navigation error: {ex.Message}");
                // 必要に応じてユーザーへの通知を実装
            }
        }
    }
}
