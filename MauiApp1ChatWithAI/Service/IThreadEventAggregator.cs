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
        event EventHandler<ChatThread> ThreadCreated;
        void PublishThreadCreated(ChatThread thread);

        event EventHandler<ChatThread> ThreadSelected;
        void PublishThreadSelected(ChatThread thread);
    }
}
