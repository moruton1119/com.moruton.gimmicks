using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // プレビュー描画・ドロップゾーン管理
    public partial class MetamorphoseWindow
    {
        private Color _lastGimmickColor;
        private float _previewRefreshTime = -1f;
        private readonly Dictionary<Object, Texture2D> _previewTexCache = new();
        private readonly HashSet<Texture2D> _ownedPreviewTextures = new();

        // ドロップゾーンとプロパティの対応
        private static readonly (string dropzoneName, string propertyPath, string previewName)[] DropzoneBindings =
        {
            ("off-dropzone", "offTargets", "off-preview"),
            ("head-dropzone", "headItems", "head-preview"),
            ("body-dropzone", "bodyItems", "body-preview"),
            ("hand-dropzone", "handItems", "hand-preview"),
            ("leg-dropzone", "legItems", "leg-preview"),
        };

        #region Preview

        private void RegisterPreviewCallbacks()
        {
            // ドロップゾーンの登録
            foreach (var (dropzoneName, propertyPath, previewName) in DropzoneBindings)
            {
                SetupDropzone(dropzoneName, propertyPath, previewName);
            }

            // 色変更コールバック
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

        private void SetupDropzone(string dropzoneName, string propertyPath, string previewName)
        {
            var dropzone = _root.Q<VisualElement>(dropzoneName);
            if (dropzone == null) return;

            // ドラッグ中のハイライト
            dropzone.RegisterCallback<DragEnterEvent>(e =>
            {
                dropzone.AddToClassList("drag-hover");
            });
            dropzone.RegisterCallback<DragLeaveEvent>(e =>
            {
                dropzone.RemoveFromClassList("drag-hover");
            });

            // ★ これがないとドラッグを受け付けてくない
            dropzone.RegisterCallback<DragUpdatedEvent>(e =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            });

            // ドロップ処理
            dropzone.RegisterCallback<DragPerformEvent>(e =>
            {
                dropzone.RemoveFromClassList("drag-hover");
                DragAndDrop.AcceptDrag();

                var draggedObjects = DragAndDrop.objectReferences;
                if (draggedObjects == null || draggedObjects.Length == 0) return;

                _so.Update();
                var prop = _so.FindProperty(propertyPath);
                if (prop == null || !prop.isArray) return;

                bool added = false;
                foreach (var obj in draggedObjects)
                {
                    GameObject go = null;
                    if (obj is GameObject directGo)
                        go = directGo;
                    else if (obj is Component comp)
                        go = comp.gameObject;

                    if (go == null) continue;

                    // 重複チェック
                    bool exists = false;
                    for (int i = 0; i < prop.arraySize; i++)
                    {
                        if (prop.GetArrayElementAtIndex(i).objectReferenceValue == go)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        prop.InsertArrayElementAtIndex(prop.arraySize);
                        prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = go;
                        added = true;
                    }
                }

                if (added)
                {
                    _so.ApplyModifiedProperties();
                    UpdatePartPreview(previewName, propertyPath);
                    _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;
                }
            });
        }

        private void UpdateAllPreviews()
        {
            foreach (var (dropzoneName, propertyPath, previewName) in DropzoneBindings)
                UpdatePartPreview(previewName, propertyPath);

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

            // ドロップゾーンのラベル更新
            var dropzoneName = GetDropzoneNameForPreview(previewElementName);
            if (dropzoneName != null)
            {
                var label = _root.Q<Label>(className: "mesh-dropzone-label");
                var dropzone = _root.Q<VisualElement>(dropzoneName);
                if (dropzone != null)
                {
                    var dropLabel = dropzone.Q<Label>(className: "mesh-dropzone-label");
                    if (dropLabel != null)
                    {
                        dropLabel.text = prop.arraySize == 0 ? "Drop meshes here" : $"{prop.arraySize} mesh(es) — drop to add";
                    }
                }
            }

            for (int i = 0; i < prop.arraySize; i++)
            {
                var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (go == null) continue;

                var previewTex = GetPreviewTexture(go);

                // プレビューアイテム（画像＋削除ボタン）
                var item = new VisualElement();
                item.AddToClassList("preview-item");

                var img = new Image();
                if (previewTex != null)
                    img.image = previewTex;
                img.AddToClassList("preview-thumb-large");
                item.Add(img);

                // 削除ボタン
                var capturedPath = propertyPath;
                var capturedIndex = i;
                var capturedPreview = previewElementName;
                var removeBtn = new Button(() =>
                {
                    _so.Update();
                    var p = _so.FindProperty(capturedPath);
                    if (p != null && p.isArray && capturedIndex < p.arraySize)
                    {
                        p.DeleteArrayElementAtIndex(capturedIndex);
                        _so.ApplyModifiedProperties();
                        UpdatePartPreview(capturedPreview, capturedPath);
                    }
                });
                removeBtn.text = "x";
                removeBtn.AddToClassList("preview-remove-btn");
                item.Add(removeBtn);

                container.Add(item);
            }
        }

        private string GetDropzoneNameForPreview(string previewName)
        {
            return previewName switch
            {
                "off-preview" => "off-dropzone",
                "head-preview" => "head-dropzone",
                "body-preview" => "body-dropzone",
                "hand-preview" => "hand-dropzone",
                "leg-preview" => "leg-dropzone",
                _ => null,
            };
        }

        private Texture2D GetPreviewTexture(GameObject go)
        {
            if (go == null) return null;

            // キャッシュチェック
            if (_previewTexCache.TryGetValue(go, out var cached))
                return cached;

            // AssetPreviewは使わない（遠すぎる）
            // 常にクローズアップレンダリングを使う
            var tex = RenderCloseUpPreview(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                _ownedPreviewTextures.Add(tex);
                return tex;
            }

            // フォールバック
            tex = AssetPreview.GetMiniThumbnail(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                return tex;
            }

            return null;
        }

        private static Texture2D RenderCloseUpPreview(GameObject sourceGo)
        {
            if (sourceGo == null) return null;

            // Rendererを取得（SkinnedMeshRenderer含む）
            var renderers = sourceGo.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return null;

            // メッシュとマテリアルを抽出
            var meshes = new System.Collections.Generic.List<(Mesh mesh, Material[] materials, Transform transform)>();

            foreach (var renderer in renderers)
            {
                if (renderer is SkinnedMeshRenderer smr)
                {
                    if (smr.sharedMesh == null) continue;
                    var bakedMesh = new Mesh();
                    smr.BakeMesh(bakedMesh, true);
                    meshes.Add((bakedMesh, smr.sharedMaterials, smr.transform));
                }
                else if (renderer is MeshRenderer mr)
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    if (mf == null || mf.sharedMesh == null) continue;
                    meshes.Add((mf.sharedMesh, mr.sharedMaterials, mr.transform));
                }
            }

            if (meshes.Count == 0) return null;

            // バウンズ計算
            var bounds = meshes[0].mesh.bounds;
            foreach (var (mesh, _, _) in meshes)
                bounds.Encapsulate(mesh.bounds);

            float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxDim <= 0f) maxDim = 1f;

            const int texSize = 128;
            var tex = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);

            var preview = new PreviewRenderUtility();
            try
            {
                preview.camera.fieldOfView = 30f;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(0, 0, 0, 0);

                float halfFov = preview.camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float dist = maxDim / (2f * Mathf.Tan(halfFov));

                Vector3 center = bounds.center;
                preview.camera.transform.position = center - Vector3.forward * dist;
                preview.camera.transform.LookAt(center);
                preview.camera.nearClipPlane = 0.001f;
                preview.camera.farClipPlane = dist + maxDim * 2f;

                // メッシュを描画
                foreach (var (mesh, materials, transform) in meshes)
                {
                    for (int sub = 0; sub < mesh.subMeshCount && sub < materials.Length; sub++)
                    {
                        if (materials[sub] == null) continue;
                        Graphics.DrawMesh(
                            mesh,
                            transform.localToWorldMatrix,
                            materials[sub],
                            0,
                            preview.camera,
                            sub
                        );
                    }
                }

                preview.camera.Render();

                // バックアップしたRenderTextureから読み取り
                var rt = RenderTexture.active;
                RenderTexture.active = preview.camera.targetTexture;
                tex.ReadPixels(new Rect(0, 0, texSize, texSize), 0, 0);
                tex.Apply();
                RenderTexture.active = rt;

                // bakedMeshをクリーンアップ
                foreach (var (mesh, _, _) in meshes)
                {
                    if (!mesh.Equals(null) && mesh.vertexCount > 0)
                        Object.DestroyImmediate(mesh);
                }

                return tex;
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
            foreach (var (_, propertyPath, _) in DropzoneBindings)
            {
                var prop = _so.FindProperty(propertyPath);
                if (prop == null || !prop.isArray) continue;

                for (int i = 0; i < prop.arraySize; i++)
                {
                    var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (go == null) continue;
                    if (!_previewTexCache.ContainsKey(go))
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
