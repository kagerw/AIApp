using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Service
{
    public interface ISecureStorageService
    {
        Task<string> GetSecureValue(string key);
        Task SetSecureValue(string key, string value);
        Task RemoveSecureValue(string key);
    }
}
