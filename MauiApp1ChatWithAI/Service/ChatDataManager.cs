using MauiApp1ChatWithAI.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MauiApp1ChatWithAI.Service
{
    /// <summary>
    /// チャットデータの永続化を担当するクラス
    /// </summary>
    public class ChatDataManager : IChatDataManager
    {
        private readonly ChatDbContext _context;

        public ChatDataManager(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateThreadAsync(
            string title,
            string provider = "Claude",
            string? systemPrompt = null,
            bool isSystemPromptEnabled = false)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));

            var thread = new ChatThread
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Provider = provider,
                SystemPrompt = systemPrompt,
                IsSystemPromptEnabled = isSystemPromptEnabled,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Threads.Add(thread);
            Debug.WriteLine($"Database path when thread was created: {_context.Database.GetDbConnection().ConnectionString}");
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return thread.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<ChatThread?> GetThreadAsync(string threadId)
        {
            if (string.IsNullOrEmpty(threadId))
                throw new ArgumentException("ThreadId cannot be empty", nameof(threadId));

            return await _context.Threads.FindAsync(threadId);
        }

        public async Task<List<ChatThread>> GetAllThreadsAsync()
        {
            return await _context.Threads
                .OrderByDescending(t => t.LastMessageAt)
                .ToListAsync();
        }

        public async Task<string> AddMessageAsync(string threadId, string role, string content)
        {
            if (string.IsNullOrEmpty(threadId))
                throw new ArgumentException("ThreadId cannot be empty", nameof(threadId));
            if (string.IsNullOrEmpty(role))
                throw new ArgumentException("Role cannot be empty", nameof(role));
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Content cannot be empty", nameof(content));
            if (role != "user" && role != "assistant")
                throw new ArgumentException("Role must be either 'user' or 'assistant'", nameof(role));

            // トランザクション開始
            using var transaction = await _context.Database.BeginTransactionAsync();

            
            try
            {
                var thread = await _context.Threads.FindAsync(threadId);
                if (thread == null)
                    throw new InvalidOperationException($"Thread with id {threadId} not found");

                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ThreadId = threadId,
                    Role = role,
                    Timestamp = DateTime.UtcNow
                };

                var element = new MessageElement
                {
                    Id = Guid.NewGuid().ToString(),
                    MessageId = message.Id,
                    Type = "Text",
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                _context.MessageElements.Add(element);

                // スレッドの最終更新時刻を更新
                thread.LastMessageAt = DateTime.UtcNow;
                _context.Threads.Update(thread);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return message.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesAsync(string threadId)
        {
            if (string.IsNullOrEmpty(threadId))
                throw new ArgumentException("ThreadId cannot be empty", nameof(threadId));

            // スレッドの存在確認
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread == null)
                throw new InvalidOperationException($"Thread with id {threadId} not found");

            return await _context.Messages
                .Where(m => m.ThreadId == threadId)
                .Include(m => m.MessageElements)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// スレッドとその関連データ（メッセージ、メッセージ要素）を削除します。
        /// </summary>
        /// <param name="threadId">削除対象のスレッドID</param>
        /// <returns>
        /// 削除が成功した場合はtrue、スレッドが存在しない場合はfalseを返します。
        /// </returns>
        /// <exception cref="ArgumentException">threadIdが空の場合にスローされます。</exception>
        /// <exception cref="InvalidOperationException">データベース操作中にエラーが発生した場合にスローされます。</exception>
        public async Task<bool> DeleteThreadAsync(string threadId)
        {
            if (string.IsNullOrEmpty(threadId))
                throw new ArgumentException("ThreadId cannot be empty", nameof(threadId));

            // トランザクション開始
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // スレッドの存在確認
                var thread = await _context.Threads.FindAsync(threadId);
                if (thread == null)
                    return false;

                // 関連するメッセージを取得
                var messages = await _context.Messages
                    .Where(m => m.ThreadId == threadId)
                    .ToListAsync();

                // メッセージIDのリストを作成
                var messageIds = messages.Select(m => m.Id).ToList();

                // 関連するメッセージ要素を取得して削除
                var elements = await _context.MessageElements
                    .Where(e => messageIds.Contains(e.MessageId))
                    .ToListAsync();
                _context.MessageElements.RemoveRange(elements);

                // メッセージを削除
                _context.Messages.RemoveRange(messages);

                // スレッドを削除
                _context.Threads.Remove(thread);

                // 変更をデータベースに保存
                await _context.SaveChangesAsync();

                // トランザクションのコミット
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                // エラーが発生した場合はロールバック
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to delete thread: {ex.Message}", ex);
            }
        }
    }

}
