using System.Reflection;

namespace MorulabTools.Launcher
{
    /// <summary>
    /// 収集した各コマンドのデータを保持するクラス
    /// </summary>
    public class ToolCommandData
    {
        public string Title;        // メニューのパスから取得した名前
        public string Path;         // 完全なメニューパス
        public string Description;  // 属性から取得した説明
        public string Category;     // 属性から取得したカテゴリ
        public string IconName;     // アイコン名
        public MethodInfo TargetMethod; // 実行対象のメソッド
    }
}
