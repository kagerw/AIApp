using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Models
{
    public class DatabaseSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString =>
            $"Server={Server};Port={Port};Database={DatabaseName};User Id={Username};Password={Password};";
    }
}
