using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Models.Database
{
    public class MigrationHistory
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public DateTime AppliedAt { get; set; }

        // オプション：マイグレーションの詳細情報
        public string Description { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
