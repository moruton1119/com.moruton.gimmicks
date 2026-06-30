using System.Collections.Generic;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// エディターテーマの登録・管理。
    /// 新しいテーマを追加する時は RegisterAll() に1行追加するだけ。
    /// </summary>
    public static class EditorThemeRegistry
    {
        private static readonly List<EditorThemeDefinition> _themes = new List<EditorThemeDefinition>();
        private static readonly Dictionary<string, EditorThemeDefinition> _themeById = new Dictionary<string, EditorThemeDefinition>();

        private static bool _initialized;

        /// <summary>登録されている全テーマ</summary>
        public static IReadOnlyList<EditorThemeDefinition> Themes
        {
            get
            {
                EnsureInitialized();
                return _themes;
            }
        }

        /// <summary>
        /// テーマIDから定義を取得。
        /// 見つからない場合は Moonlight を返す（フォールバック）。
        /// </summary>
        public static EditorThemeDefinition GetTheme(string id)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(id) && _themeById.TryGetValue(id, out var def))
                return def;
            return EditorThemeDefinition.Moonlight;
        }

        /// <summary>
        /// テーマが存在するか確認。
        /// </summary>
        public static bool HasTheme(string id)
        {
            EnsureInitialized();
            return !string.IsNullOrEmpty(id) && _themeById.ContainsKey(id);
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            RegisterAll();
        }

        /// <summary>
        /// テーマを登録。
        /// 新テーマを追加する時はここに1行追加する。
        /// </summary>
        private static void RegisterAll()
        {
            Register(EditorThemeDefinition.Moonlight);
            Register(EditorThemeDefinition.Daylight);
            Register(EditorThemeDefinition.Cyber);
        }

        private static void Register(EditorThemeDefinition theme)
        {
            _themes.Add(theme);
            _themeById[theme.id] = theme;
        }
    }
}
