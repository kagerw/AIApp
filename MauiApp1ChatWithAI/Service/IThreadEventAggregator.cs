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
        event EventHandler<ChatThread> ThreadCreated;
        event EventHandler ThreadsNeedReorder;  // 追加
        event EventHandler<ChatThread> ThreadDeleted;

        void PublishThreadSelected(ChatThread thread);
        void PublishThreadCreated(ChatThread thread);
        void PublishThreadsNeedReorder();  // 追加

        void publishThreadDeleeted(ChatThread thread);
    }
}
