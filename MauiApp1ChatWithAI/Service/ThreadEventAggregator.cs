using MauiApp1ChatWithAI.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public class ThreadEventAggregator : IThreadEventAggregator
    {
        public event EventHandler<ChatThread> ThreadCreated;
        public void PublishThreadCreated(ChatThread thread) => ThreadCreated?.Invoke(this, thread);

        public event EventHandler<ChatThread> ThreadSelected;
        public event EventHandler ThreadsNeedReorder;

        public void PublishThreadSelected(ChatThread thread) => ThreadSelected?.Invoke(this, thread);

        public void PublishThreadsNeedReorder() => ThreadsNeedReorder?.Invoke(this, new EventArgs());
    }
}
