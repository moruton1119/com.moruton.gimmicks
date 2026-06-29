using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // ページ遷移・部位トグル・プロパティフィールド生成・D&D
    public partial class MetamorphoseWindow
    {
        #region Navigation

        private void SetupNavigation()
        {
            // Tab切り替え: Main ↔ Colabo
            var tabMain = _root.Q<Button>("tab-main");
            var tabColabo = _root.Q<Button>("tab-colabo");

            if (tabMain != null)
                tabMain.clicked += () => SwitchToMain();

            if (tabColabo != null)
                tabColabo.clicked += () => SwitchToColabo();

            // Dev ボタンでメインページ ↔ Devページを切替
            var devBtn = _root.Q<Button>("nav-3");
            if (devBtn != null)
                devBtn.clicked += () =>
                {
                    bool isDev = _currentPage == 3;
                    if (isDev)
                    {
                        ShowPage("page-3", "page-main", false);
                        _currentPage = 0;
                        UpdateTabActive(true);
                    }
                    else
                    {
                        ShowPage(_currentPage == 0 ? "page-main" : "page-colabo", "page-3", true);
                        _currentPage = 3;
                        UpdateTabActive(false);
                    }
                };
        }

        private void SwitchToMain()
        {
            if (_currentPage == 3)
            {
                ShowPage("page-3", "page-main", false);
            }
            else
            {
                ShowPage("page-colabo", "page-main", false);
            }
            _currentPage = 0;
            UpdateTabActive(true);
        }

        private void SwitchToColabo()
        {
            if (_currentPage == 3)
            {
                ShowPage("page-3", "page-colabo", false);
            }
            else
            {
                ShowPage("page-main", "page-colabo", false);
            }
            _currentPage = 1;
            UpdateTabActive(false);
        }

        private void UpdateTabActive(bool mainActive)
        {
            var tabMain = _root.Q<Button>("tab-main");
            var tabColabo = _root.Q<Button>("tab-colabo");
            if (tabMain != null) tabMain.EnableInClassList("tab-active", mainActive);
            if (tabColabo != null) tabColabo.EnableInClassList("tab-active", !mainActive);

            // タブバーの表示切替（Devページでは隠す）
            var tabBar = _root.Q<VisualElement>("tab-bar");
            if (tabBar != null)
                tabBar.style.display = _currentPage == 3 ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void ShowPage(string hidePage, string showPage, bool show)
        {
            var hide = _root.Q<VisualElement>(hidePage);
            if (hide != null) hide.style.display = DisplayStyle.None;
            var showEl = _root.Q<VisualElement>(showPage);
            if (showEl != null) showEl.style.display = DisplayStyle.Flex;
        }

        #endregion

        #region Drag & Drop Setup

        /// <summary>
        /// 全ドラッグ&ドロップエリアにD&Dハンドラを登録する。
        /// GameObjectがドロップされたら対応する配列プロパティに追加。
        /// </summary>
        private void SetupDragAndDrop()
        {
            // OffTargets
            SetupDragDropForSlot("page0-slot-offTargets", "offTargets");

            // 部位ごとのアイテム
            SetupDragDropForSlot("page0-slot-headItems", "headItems");
            SetupDragDropForSlot("page0-slot-bodyItems", "bodyItems");
            SetupDragDropForSlot("page0-slot-handItems", "handItems");
            SetupDragDropForSlot("page0-slot-legItems", "legItems");

            // フェード演出アイテム
            SetupDragDropForSlot("page2-slot-fadeHeadItems", "fadeHeadItems");
            SetupDragDropForSlot("page2-slot-fadeBodyItems", "fadeBodyItems");
            SetupDragDropForSlot("page2-slot-fadeArmItems", "fadeArmItems");
            SetupDragDropForSlot("page2-slot-fadeLegItems", "fadeLegItems");
        }

        private void SetupDragDropForSlot(string slotName, string propertyPath)
        {
            var slot = _root.Q<VisualElement>(slotName);
            if (slot == null) return;

            // ドラッグ中のハイライト
            slot.RegisterCallback<DragEnterEvent>(e =>
            {
                slot.AddToClassList("drag-drop-hover");
            });
            slot.RegisterCallback<DragLeaveEvent>(e =>
            {
                slot.RemoveFromClassList("drag-drop-hover");
            });

            // ドロップ処理
            slot.RegisterCallback<DragPerformEvent>(e =>
            {
                slot.RemoveFromClassList("drag-drop-hover");

                if (_target == null || _so == null) return;

                // ドラッグされたオブジェクトを取得
                var draggedObjects = DragAndDrop.objectReferences;
                if (draggedObjects == null || draggedObjects.Length == 0) return;

                _so.Update();
                var prop = _so.FindProperty(propertyPath);
                if (prop == null || !prop.isArray) return;

                foreach (var obj in draggedObjects)
                {
                    GameObject go = null;

                    // GameObject直接
                    if (obj is GameObject directGo)
                        go = directGo;
                    // Prefabやアセットの場合
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
                    }
                }

                _so.ApplyModifiedProperties();

                // プレビュー更新
                var previewName = GetPreviewNameForSlot(slotName);
                if (previewName != null)
                    UpdatePartPreview(previewName, propertyPath);

                _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;
            });
        }

        private string GetPreviewNameForSlot(string slotName)
        {
            return slotName switch
            {
                "page0-slot-headItems" => "head-preview",
                "page0-slot-bodyItems" => "body-preview",
                "page0-slot-handItems" => "hand-preview",
                "page0-slot-legItems" => "leg-preview",
                _ => null,
            };
        }

        #endregion

        #region Part Toggles

        private void SetupPartToggles()
        {
            SetupPartToggle("toggle-head", "section-head", "Head", () => _target.showHead, "showHead");
            SetupPartToggle("toggle-body", "section-body", "Body", () => _target.showBody, "showBody");
            SetupPartToggle("toggle-hand", "section-hand", "Hand", () => _target.showHand, "showHand");
            SetupPartToggle("toggle-leg", "section-leg", "Leg", () => _target.showLeg, "showLeg");
            SetupPartToggle("toggle-fadeHead", "section-fadeHead", "F-Head", () => _target.showFadeHead, "showFadeHead");
            SetupPartToggle("toggle-fadeBody", "section-fadeBody", "F-Body", () => _target.showFadeBody, "showFadeBody");
            SetupPartToggle("toggle-fadeArm", "section-fadeArm", "F-Arm", () => _target.showFadeArm, "showFadeArm");
            SetupPartToggle("toggle-fadeLeg", "section-fadeLeg", "F-Leg", () => _target.showFadeLeg, "showFadeLeg");
        }

        private void SetupPartToggle(string toggleName, string sectionName, string displayName, System.Func<bool> getter, string propertyPath)
        {
            var toggle = _root.Q<Button>(toggleName);
            var section = _root.Q<VisualElement>(sectionName);
            if (toggle == null || section == null) return;

            bool isVisible = getter();
            ApplyPartToggleVisual(toggle, section, displayName, isVisible);

            toggle.clicked += () =>
            {
                isVisible = !isVisible;

                _so.Update();
                var prop = _so.FindProperty(propertyPath);
                if (prop != null)
                {
                    prop.boolValue = isVisible;
                    _so.ApplyModifiedProperties();
                }
                else
                {
                    Undo.RecordObject(_target, "Toggle Part Visibility");
                    switch (propertyPath)
                    {
                        case "showHead": _target.showHead = isVisible; break;
                        case "showBody": _target.showBody = isVisible; break;
                        case "showHand": _target.showHand = isVisible; break;
                        case "showLeg": _target.showLeg = isVisible; break;
                        case "showFadeHead": _target.showFadeHead = isVisible; break;
                        case "showFadeBody": _target.showFadeBody = isVisible; break;
                        case "showFadeArm": _target.showFadeArm = isVisible; break;
                        case "showFadeLeg": _target.showFadeLeg = isVisible; break;
                    }
                    EditorUtility.SetDirty(_target);
                }

                ApplyPartToggleVisual(toggle, section, displayName, isVisible);
            };
        }

        private void ApplyPartToggleVisual(Button toggle, VisualElement section, string displayName, bool isVisible)
        {
            toggle.text = $"{displayName}: {(isVisible ? "ON" : "OFF")}";
            toggle.EnableInClassList("part-toggle-off", !isVisible);
            section.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #endregion

        #region PropertyField Generation

        private void CreatePropertyFields()
        {
            for (int pageIdx = 0; pageIdx < PageFieldPaths.Length; pageIdx++)
            {
                foreach (var path in PageFieldPaths[pageIdx])
                {
                    var slot = _root.Q<VisualElement>($"page{pageIdx}-slot-{path}");
                    if (slot == null) continue;
                    slot.Add(new PropertyField { bindingPath = path });
                }
            }

            foreach (var (path, slotName) in DevFieldBindings)
            {
                var slot = _root.Q<VisualElement>(slotName);
                if (slot == null) continue;
                slot.Add(new PropertyField { bindingPath = path });
            }
        }

        #endregion
    }
}
