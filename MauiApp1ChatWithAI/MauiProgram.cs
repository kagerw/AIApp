using CommunityToolkit.Maui;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using MauiApp1ChatWithAI.ViewModels;
using MauiApp1ChatWithAI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MauiApp1ChatWithAI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");
            builder.Services.AddDbContext<ChatDbContext>(options =>
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");

                Debug.WriteLine("dbPath is " + dbPath + Environment.NewLine);

                options.UseSqlite($"Data Source={dbPath}");
            });

            // HttpClientの登録
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IThreadEventAggregator, ThreadEventAggregator>();
            // サービスの登録
            builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
            builder.Services.AddSingleton<IChatDataManager, ChatDataManager>();
            builder.Services.AddSingleton<ILLMApiService, ClaudeApiService>();
            builder.Services.AddSingleton<ChatService>();

            // ViewModels and Pages
            builder.Services.AddTransient<TestViewModel>();
            builder.Services.AddTransient<TestPage>();

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<DevMenuViewModel>();
            builder.Services.AddTransient<ThreadSettingsViewModel>();

            // Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<DevMenuPage>();
            builder.Services.AddTransient<ThreadSettingsPage>();

            builder.Services.AddTransient<ThreadListViewModel>();
            builder.Services.AddTransient<ThreadListControl>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
