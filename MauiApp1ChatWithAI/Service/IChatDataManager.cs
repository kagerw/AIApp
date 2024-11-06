using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    /// <summary>
    /// チャットアプリケーションのデータ操作を提供するインターフェース。
    /// スレッドの作成・取得、メッセージの追加・取得などの基本的なデータ操作を行います。
    /// </summary>
    public interface IChatDataManager
    {
        /// <summary>
        /// 新しいチャットスレッドを作成します。
        /// </summary>
        /// <param name="title">スレッドのタイトル。必須項目です。</param>
        /// <param name="provider">使用するAIプロバイダー。指定がない場合は"Claude"が使用されます。</param>
        /// <param name="systemPrompt">AIに対する初期指示。nullの場合、システムプロンプトは設定されません。</param>
        /// <param name="isSystemPromptEnabled">システムプロンプトの有効/無効を指定。デフォルトはfalse。</param>
        /// <returns>作成されたスレッドの一意のID。</returns>
        /// <exception cref="ArgumentException">titleが空の場合にスローされます。</exception>
        /// <exception cref="InvalidOperationException">データベースへの保存に失敗した場合にスローされます。</exception>
        Task<string> CreateThreadAsync(
            string title,
            string provider = "Claude",
            string? systemPrompt = null,
            bool isSystemPromptEnabled = false);

        /// <summary>
        /// 指定されたIDのスレッドを取得します。
        /// </summary>
        /// <param name="threadId">取得するスレッドのID。</param>
        /// <returns>
        /// 指定されたIDのスレッド。
        /// スレッドが存在しない場合はnullを返します。
        /// </returns>
        /// <exception cref="ArgumentException">threadIdが空の場合にスローされます。</exception>
        Task<ChatThread?> GetThreadAsync(string threadId);

        /// <summary>
        /// すべてのスレッドを取得します。
        /// スレッドは最終更新日時の降順（新しい順）でソートされます。
        /// </summary>
        /// <returns>
        /// スレッドのリスト。
        /// スレッドが存在しない場合は空のリストを返します。
        /// </returns>
        Task<List<ChatThread>> GetAllThreadsAsync();

        /// <summary>
        /// 指定されたスレッドに新しいメッセージを追加します。
        /// </summary>
        /// <param name="threadId">メッセージを追加するスレッドのID。</param>
        /// <param name="role">メッセージの送信者の役割。"user"または"assistant"を指定します。</param>
        /// <param name="content">メッセージの内容。</param>
        /// <returns>作成されたメッセージの一意のID。</returns>
        /// <exception cref="ArgumentException">
        /// - threadIdが空の場合
        /// - roleが"user"または"assistant"以外の場合
        /// - contentが空の場合
        /// にスローされます。
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// - 指定されたスレッドが存在しない場合
        /// - データベースへの保存に失敗した場合
        /// にスローされます。
        /// </exception>
        Task<string> AddMessageAsync(string threadId, string role, string content);

        /// <summary>
        /// 指定されたスレッドのすべてのメッセージを取得します。
        /// メッセージは時系列順（古い順）でソートされます。
        /// </summary>
        /// <param name="threadId">メッセージを取得するスレッドのID。</param>
        /// <returns>
        /// メッセージのリスト。メッセージが存在しない場合は空のリストを返します。
        /// 各メッセージにはMessageElementsコレクションが含まれます。
        /// </returns>
        /// <exception cref="ArgumentException">threadIdが空の場合にスローされます。</exception>
        /// <exception cref="InvalidOperationException">指定されたスレッドが存在しない場合にスローされます。</exception>
        Task<List<Message>> GetMessagesAsync(string threadId);
    }

}
