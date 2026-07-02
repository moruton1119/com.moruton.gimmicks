#if MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// EncryptedAnimData.dllからアニメーションを復号して読み込む。
    /// 鍵の扱いはDLL内部で完結するため、このクラスは鍵を知らない。
    /// </summary>
    public static class ProtectedAnimLoader
    {
        private static Assembly _dllAssembly;
        private static Type _dataType;
        private static MethodInfo _getDataMethod;
        private static MethodInfo _getKeysMethod;
        private static string _loadedDllPath;

        /// <summary>
        /// 指定されたDLLを読み込む。
        /// 既に同じパスのDLLが読み込まれている場合はスキップ。
        /// </summary>
        public static bool LoadDll(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            {
                Debug.LogWarning("[ProtectedAnimLoader] DLL not found: " + dllPath);
                return false;
            }

            // 既に同じDLLが読み込まれている
            if (_dllAssembly != null && _loadedDllPath == dllPath)
                return true;

            try
            {
                byte[] dllBytes = File.ReadAllBytes(dllPath);
                _dllAssembly = Assembly.Load(dllBytes);
                _loadedDllPath = dllPath;

                // ProtectedAssets.EncryptedAnimData クラスを探す
                _dataType = _dllAssembly.GetType("ProtectedAssets.EncryptedAnimData");

                if (_dataType == null)
                {
                    Debug.LogError("[ProtectedAnimLoader] ProtectedAssets.EncryptedAnimData type not found in DLL.");
                    _dllAssembly = null;
                    return false;
                }

                _getDataMethod = _dataType.GetMethod("GetDecryptedData", BindingFlags.Public | BindingFlags.Static);
                _getKeysMethod = _dataType.GetMethod("GetAvailableKeys", BindingFlags.Public | BindingFlags.Static);

                if (_getDataMethod == null)
                {
                    Debug.LogError("[ProtectedAnimLoader] GetDecryptedData method not found.");
                    _dllAssembly = null;
                    return false;
                }

                Debug.Log($"[ProtectedAnimLoader] DLL loaded: {dllPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtectedAnimLoader] Failed to load DLL: {e.Message}");
                _dllAssembly = null;
                return false;
            }
        }

        /// <summary>
        /// 指定キーの復号済みバイナリデータを取得。
        /// 鍵はDLL内部で完結（このクラスは鍵を知らない）。
        /// </summary>
        public static byte[] LoadDecrypted(string animKey)
        {
            if (_dataType == null || _getDataMethod == null)
            {
                Debug.LogWarning("[ProtectedAnimLoader] DLL not loaded. Call LoadDll first.");
                return null;
            }

            try
            {
                var result = _getDataMethod.Invoke(null, new object[] { animKey });
                return result as byte[];
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProtectedAnimLoader] Failed to decrypt '{animKey}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// DLL内の利用可能なキー一覧を取得。
        /// </summary>
        public static string[] GetAvailableKeys()
        {
            if (_getKeysMethod == null)
                return Array.Empty<string>();

            try
            {
                var result = _getKeysMethod.Invoke(null, null);
                return result as string[] ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// DLLが読み込まれているか。
        /// </summary>
        public static bool IsLoaded => _dllAssembly != null;

        /// <summary>
        /// DefaultAsset（DLL）からファイルパスを取得。
        /// </summary>
        public static string GetDllPath(Object dllAsset)
        {
            if (dllAsset == null) return null;
            return AssetDatabase.GetAssetPath(dllAsset);
        }
    }
}
#endif
