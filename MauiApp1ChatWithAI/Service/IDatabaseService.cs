using MauiApp1ChatWithAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1ChatWithAI.Services
{
    /// <summary>
    /// データベース設定のストレージと接続テストを管理するサービスのインターフェース
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// データベース接続のテストを実行します
        /// </summary>
        /// <param name="settings">テストする接続設定</param>
        /// <returns>接続が成功した場合はtrue、失敗した場合はfalse</returns>
        /// <exception cref="ArgumentNullException">settingsがnullの場合にスローされます</exception>
        Task<bool> TestConnectionAsync(DatabaseSettings settings);

        /// <summary>
        /// データベース設定を保存します
        /// </summary>
        /// <param name="settings">保存する設定</param>
        /// <returns>保存処理の完了を表すTask</returns>
        /// <exception cref="ArgumentNullException">settingsがnullの場合にスローされます</exception>
        /// <exception cref="ApplicationException">設定の保存に失敗した場合にスローされます</exception>
        Task SaveSettingsAsync(DatabaseSettings settings);

        /// <summary>
        /// 保存されているデータベース設定を読み込みます
        /// </summary>
        /// <returns>データベース設定。設定が存在しない場合はデフォルト値が設定されたインスタンス</returns>
        /// <exception cref="ApplicationException">設定の読み込みに失敗した場合にスローされます</exception>
        Task<DatabaseSettings> LoadSettingsAsync();
    }
}
