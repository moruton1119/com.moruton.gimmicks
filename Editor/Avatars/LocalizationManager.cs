using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 多言語対応を管理する共通マネージャー
    /// JSONファイルからテキストを読み込み、キャッシュします
    /// </summary>
    public static class LocalizationManager
    {
        private const string LocalizationBasePath = "Packages/com.moruton.gimmicks/Editor/Avatars/Localization";
        
        private static Dictionary<string, Dictionary<string, string>> commonTexts 
            = new Dictionary<string, Dictionary<string, string>>();
        
        private static Dictionary<string, Dictionary<string, string>> scriptTexts 
            = new Dictionary<string, Dictionary<string, string>>();
        
        private static string[] supportedLanguageCodes = { "ja", "en", "ko", "it", "es" };
        private static string[] supportedLanguageNames = { "日本語", "English", "한국어", "Italiano", "Español" };
        
        private static string currentLanguage = "ja";
        
        public static string[] SupportedLanguageNames => supportedLanguageNames;
        public static string[] SupportedLanguageCodes => supportedLanguageCodes;
        public static string CurrentLanguage => currentLanguage;
        
        public static void SetLanguage(string languageCode)
        {
            if (Array.IndexOf(supportedLanguageCodes, languageCode) == -1)
            {
                Debug.LogWarning($"Unsupported language code: {languageCode}. Defaulting to 'ja'");
                languageCode = "ja";
            }
            currentLanguage = languageCode;
        }
        
        public static void Load(string scriptName, string languageCode)
        {
            SetLanguage(languageCode);
            
            // 共通テキスト読み込み
            if (!commonTexts.ContainsKey(languageCode))
            {
                commonTexts[languageCode] = LoadJson(Path.Combine(LocalizationBasePath, "Common", $"{languageCode}.json"));
            }
            
            // スクリプト固有テキスト読み込み
            string scriptKey = $"{scriptName}_{languageCode}";
            if (!scriptTexts.ContainsKey(scriptKey))
            {
                scriptTexts[scriptKey] = LoadJson(Path.Combine(LocalizationBasePath, scriptName, $"{languageCode}.json"));
            }
        }
        
        public static string Get(string scriptName, string key)
        {
            string scriptKey = $"{scriptName}_{currentLanguage}";
            
            // スクリプト固有テキストを検索
            if (scriptTexts.TryGetValue(scriptKey, out var scriptDict))
            {
                if (scriptDict.TryGetValue(key, out var text))
                {
                    return text;
                }
            }
            
            // 共通テキストを検索
            if (commonTexts.TryGetValue(currentLanguage, out var commonDict))
            {
                if (commonDict.TryGetValue(key, out var text))
                {
                    return text;
                }
            }
            
            // 見つからない場合はキーをそのまま返す
            return key;
        }
        
        public static string GetCommon(string key)
        {
            if (commonTexts.TryGetValue(currentLanguage, out var dict))
            {
                if (dict.TryGetValue(key, out var text))
                {
                    return text;
                }
            }
            return key;
        }
        
        private static string GetAbsolutePath(string packageRelativePath)
        {
            if (packageRelativePath.StartsWith("Packages/"))
            {
                var segments = packageRelativePath.Split(new[] { '/' }, 4);
                if (segments.Length >= 4)
                {
                    string relativeInPackage = segments[2] + "/" + segments[3];
                    var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(LocalizationManager).Assembly);
                    if (packageInfo != null)
                    {
                        return Path.Combine(packageInfo.resolvedPath, relativeInPackage);
                    }
                }
            }
            return packageRelativePath;
        }
        
        private static Dictionary<string, string> LoadJson(string path)
        {
            var result = new Dictionary<string, string>();
            
            string absolutePath = GetAbsolutePath(path);
            
            if (!File.Exists(absolutePath))
            {
                Debug.LogWarning($"Localization file not found: {absolutePath}");
                return result;
            }
            
            try
            {
                string json = File.ReadAllText(absolutePath);
                var wrapper = JsonUtility.FromJson<LocalizationData>(json);
                
                if (wrapper.items != null)
                {
                    foreach (var item in wrapper.items)
                    {
                        result[item.key] = item.value;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load localization file {path}: {e.Message}");
            }
            
            return result;
        }
        
        [Serializable]
        private class LocalizationData
        {
            public List<LocalizationItem> items;
        }
        
        [Serializable]
        private class LocalizationItem
        {
            public string key;
            public string value;
        }
    }
}
