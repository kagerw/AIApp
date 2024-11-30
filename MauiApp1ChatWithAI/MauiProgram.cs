using CommunityToolkit.Maui;
using MauiApp1ChatWithAI.Models;
using MauiApp1ChatWithAI.Models.Database;
using MauiApp1ChatWithAI.Service;
using MauiApp1ChatWithAI.Services;
using MauiApp1ChatWithAI.ViewModels;
using MauiApp1ChatWithAI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();


            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "chat.db");
            builder.Services.AddDbContext<ChatDbContext>(async (serviceProvider, options) =>
            {
                // DI コンテナから IDatabaseService を取得
                var databaseService = serviceProvider.GetRequiredService<IDatabaseService>();

                // 非同期で設定を取得
                var settings = await databaseService.LoadSettingsAsync();

                // 接続文字列の初期化
                string connectionString;

                if (settings== null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    Debug.WriteLine("データベース設定が見つかりません。デフォルトのSQLite接続文字列を使用します。");

                    // デフォルトのSQLite接続文字列を設定
                    var dbPath = Path.Combine(FileSystem.AppDataDirectory, "default_chat.db");
                    connectionString = $"Data Source={dbPath}";

                    Debug.WriteLine("SQLiteのdbPathは " + dbPath + Environment.NewLine);

                    // SQLiteを使用してDbContextのオプションを設定
                    options.UseSqlite(connectionString);
                }
                else
                {
                    // 設定から接続文字列を取得
                    connectionString = settings.ConnectionString;

                    Debug.WriteLine("Using database connection string: " + connectionString);

                    // MySQLを使用してDbContextのオプションを設定
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                }
            });

            // HttpClientの登録
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IThreadEventAggregator, ThreadEventAggregator>();
            // サービスの登録
            builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
            builder.Services.AddSingleton<IChatDataManager, ChatDataManager>();
            builder.Services.AddSingleton<ILLMApiService, ClaudeApiService>();
            builder.Services.AddSingleton<ChatService>();

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<DevMenuViewModel>();
            builder.Services.AddTransient<ThreadCreateViewModel>();
            builder.Services.AddTransient<ThreadEditViewModel>();

            // Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<DevMenuPage>();
            builder.Services.AddTransient<ThreadCreatePage>();

            builder.Services.AddTransient<ThreadListViewModel>();
            builder.Services.AddTransient<ThreadListControl>();
            builder.Services.AddTransient<ThreadEditPage>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();

            


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
