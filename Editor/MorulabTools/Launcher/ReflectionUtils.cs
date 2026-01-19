using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace MorulabTools.Launcher
{
    // --- Attributes ---

    /// <summary>
    /// 各言語ごとのタイトルと説明文を定義します。
    /// lang: "en", "ja", "ko" など
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ToolLocalizeAttribute : Attribute
    {
        public string Lang { get; }
        public string Title { get; }
        public string Description { get; }
        public string Category { get; }

        public ToolLocalizeAttribute(string lang, string title, string description, string category = null)
        {
            Lang = lang;
            Title = title;
            Description = description;
            Category = category;
        }
    }

    /// <summary>
    /// (Legacy) 旧来の属性。デフォルト(en)として扱います。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MenuDescriptionAttribute : Attribute
    {
        public string Description { get; }
        public string Category { get; }
        public string IconName { get; }

        public MenuDescriptionAttribute(string description, string category = "General", string iconName = null)
        {
            Description = description;
            Category = category;
            IconName = iconName;
        }
    }

    // --- Data Classes ---

    public class ToolCommandData
    {
        public string Path;        // MenuItem path
        public string OriginalTitle; // Fallback
        
        // 言語ごとのデータ (Key: "ja", "en", "ko")
        public Dictionary<string, LocalizedInfo> LocalizedInfos = new Dictionary<string, LocalizedInfo>();

        public MethodInfo TargetMethod;
        public string IconName;

        // 指定言語の情報を取得するヘルパー
        public LocalizedInfo GetInfo(string lang)
        {
            if (LocalizedInfos.TryGetValue(lang, out var info)) return info;
            if (LocalizedInfos.TryGetValue("en", out var enInfo)) return enInfo; // Fallback to EN
            
            // 最後の手段: 自動生成
            return new LocalizedInfo 
            { 
                Title = OriginalTitle, 
                Description = "No description.", 
                Category = "General" 
            };
        }
    }

    public class LocalizedInfo
    {
        public string Title;
        public string Description;
        public string Category;
    }

    // --- Utils ---

    public static class ReflectionUtils
    {
        public static List<ToolCommandData> FindCommands(string rootPathFilter = "Morulab")
        {
            var commands = new List<ToolCommandData>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException) { continue; }

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var method in methods)
                    {
                        var menuItemAttrs = method.GetCustomAttributes<MenuItem>(false);
                        var descAttr = method.GetCustomAttribute<MenuDescriptionAttribute>(false);
                        var locAttrs = method.GetCustomAttributes<ToolLocalizeAttribute>(false);

                        foreach (var menuItemAttr in menuItemAttrs)
                        {
                            string menuPath = menuItemAttr.menuItem;
                            if (menuItemAttr.validate) continue;

                            if (!string.IsNullOrEmpty(rootPathFilter) && !menuPath.StartsWith(rootPathFilter))
                            {
                                continue;
                            }

                            string relativePath = menuPath;
                            if (menuPath.StartsWith(rootPathFilter))
                            {
                                relativePath = menuPath.Substring(rootPathFilter.Length).TrimStart('/');
                            }

                            var parts = relativePath.Split('/');
                            string autoCategory = "General";
                            if (parts.Length > 1) autoCategory = parts[0];
                            string autoTitle = parts.Last();

                            var cmd = new ToolCommandData
                            {
                                Path = menuPath,
                                OriginalTitle = autoTitle,
                                TargetMethod = method,
                                IconName = descAttr?.IconName
                            };

                            // Default (EN) from MenuDescription or Auto
                            cmd.LocalizedInfos["en"] = new LocalizedInfo
                            {
                                Title = autoTitle,
                                Description = descAttr?.Description ?? "No description available.",
                                Category = descAttr?.Category ?? autoCategory
                            };

                            // Multi-lang overrides
                            foreach (var attr in locAttrs)
                            {
                                cmd.LocalizedInfos[attr.Lang] = new LocalizedInfo
                                {
                                    Title = attr.Title,
                                    Description = attr.Description,
                                    Category = attr.Category ?? cmd.LocalizedInfos["en"].Category
                                };
                            }

                            commands.Add(cmd);
                        }
                    }
                }
            }
            // 並び替えはUI側で言語決定後に行うのがベターだが、ここではデフォルト(EN)順で返す
            return commands.OrderBy(c => c.GetInfo("en").Category).ThenBy(c => c.GetInfo("en").Title).ToList();
        }
    }
}
