using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Extensions
{
    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                // エラーログ出力などの処理
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"エラー in FireAndForgetSafe: {ex}");
#endif
            }
        }
    }
}
