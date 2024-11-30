using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public interface IThreadEventAggregator
    {
        event EventHandler<ChatThread> ThreadSelected;
        event EventHandler<string> ThreadCreated;
        event EventHandler ThreadsNeedReorder;  // 追加
        event EventHandler<ChatThread> ThreadDeleted;
        event EventHandler<ChatThread> ThreadUpdated;

        void PublishThreadSelected(ChatThread thread);
        void PublishThreadCreated(string threadId);
        void PublishThreadsNeedReorder();  // 追加

        void PublishThreadDeleeted(ChatThread thread);

        void PublishThreadUpdated(ChatThread thread);
    }
}
