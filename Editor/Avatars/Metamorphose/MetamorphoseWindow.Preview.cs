using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // プレビュー描画
    public partial class MetamorphoseWindow
    {
        private Color _lastGimmickColor;
        private float _previewRefreshTime = -1f;
        private readonly Dictionary<Object, Texture2D> _previewTexCache = new();
        private readonly HashSet<Texture2D> _ownedPreviewTextures = new();

        #region Preview

        private void RegisterPreviewCallbacks()
        {
            foreach (var (path, previewName) in PreviewBindings)
            {
                var slot = _root.Q<VisualElement>($"page0-slot-{path}");
                if (slot == null) continue;

                var pf = slot.Q<PropertyField>();
                if (pf == null) continue;

                var capturedPath = path;
                var capturedPreview = previewName;

                pf.RegisterValueChangeCallback(evt =>
                {
                    _so.Update();
                    UpdatePartPreview(capturedPreview, capturedPath);
                    _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;
                });
            }

            var colorSlot = _root.Q<VisualElement>("page0-slot-gimmickColor");
            if (colorSlot != null)
            {
                var pf = colorSlot.Q<PropertyField>();
                if (pf != null)
                {
                    pf.RegisterValueChangeCallback(evt =>
                    {
                        _so.ApplyModifiedProperties();
                        MetamorphoseSetupService.ApplyGimmickColor(_target);
                    });
                }
            }
        }

        private void UpdateAllPreviews()
        {
            foreach (var (path, previewName) in PreviewBindings)
                UpdatePartPreview(previewName, path);

            var colaboContainer = _root.Q<VisualElement>("colabo-image-container");
            if (colaboContainer != null)
            {
                colaboContainer.Clear();
                if (_target.colaboShopTex != null)
                {
                    var img = new Image { image = _target.colaboShopTex };
                    img.AddToClassList("colabo-shop-image");
                    colaboContainer.Add(img);
                }
            }
        }

        private void UpdatePartPreview(string previewElementName, string propertyPath)
        {
            var container = _root.Q<VisualElement>(previewElementName);
            if (container == null) return;

            container.Clear();

            var prop = _so.FindProperty(propertyPath);
            if (prop == null || !prop.isArray) return;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (go == null) continue;

                var previewTex = GetPreviewTexture(go);

                var img = new Image();
                if (previewTex != null)
                    img.image = previewTex;
                img.AddToClassList("preview-thumb");
                container.Add(img);
            }
        }

        private Texture2D GetPreviewTexture(GameObject go)
        {
            if (go == null) return null;

            var tex = AssetPreview.GetAssetPreview(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                return tex;
            }

            if (_previewTexCache.TryGetValue(go, out var cached))
                return cached;

            tex = AssetPreview.GetMiniThumbnail(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                return tex;
            }

            tex = RenderCloseUpPreview(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                _ownedPreviewTextures.Add(tex);
                return tex;
            }

            return null;
        }

        private static Texture2D RenderCloseUpPreview(GameObject prefabAsset)
        {
            if (prefabAsset == null) return null;

            var renderers = prefabAsset.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return null;

            var preview = new PreviewRenderUtility();
            try
            {
                preview.AddSingleGO(prefabAsset);

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (maxDim <= 0f) maxDim = 1f;

                float halfFov = preview.camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                // より近づける（0.75 → 0.65）
                float dist = maxDim / (2f * Mathf.Tan(halfFov)) * 0.65f;

                Vector3 center = bounds.center;
                preview.camera.transform.position = center - Vector3.forward * dist;
                preview.camera.transform.LookAt(center);
                preview.camera.nearClipPlane = 0.001f;
                preview.camera.farClipPlane = dist + maxDim * 2f;

                preview.camera.Render();
                return preview.EndPreview() as Texture2D;
            }
            catch
            {
                return null;
            }
            finally
            {
                preview.Cleanup();
            }
        }

        private bool HasAnyMissingPreview()
        {
            foreach (var (path, _) in PreviewBindings)
            {
                var prop = _so.FindProperty(path);
                if (prop == null || !prop.isArray) continue;

                for (int i = 0; i < prop.arraySize; i++)
                {
                    var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (go == null) continue;
                    if (AssetPreview.GetAssetPreview(go) == null && !_previewTexCache.ContainsKey(go))
                        return true;
                }
            }
            return false;
        }

        private void ClearPreviewCache()
        {
            foreach (var tex in _ownedPreviewTextures)
            {
                if (tex != null) Object.DestroyImmediate(tex);
            }
            _ownedPreviewTextures.Clear();
            _previewTexCache.Clear();
        }

        #endregion
    }
}
