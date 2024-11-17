using MauiApp1ChatWithAI.Models.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            try
            {
                await _context.SaveChangesAsync();
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
    }

}
