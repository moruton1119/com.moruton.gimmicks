using System.Collections.Generic;
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

            // タブバーの表示切替（Devページまたはコラボタブ無効時は隠す）
            var tabBar = _root.Q<VisualElement>("tab-bar");
            if (tabBar != null)
                tabBar.style.display = (_currentPage == 3 || !_target.showColaboTab) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void UpdateColaboTabVisibility(bool visible)
        {
            var tabBar = _root.Q<VisualElement>("tab-bar");
            if (tabBar != null)
            {
                tabBar.style.display = (_currentPage == 3 || !visible) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            // コラボタブが非表示の時、メインページに強制切替
            if (!visible && _currentPage == 1)
            {
                SwitchToMain();
            }

            // タブボタンの表示
            var tabColabo = _root.Q<Button>("tab-colabo");
            if (tabColabo != null)
            {
                tabColabo.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void ShowPage(string hidePage, string showPage, bool show)
        {
            var hide = _root.Q<VisualElement>(hidePage);
            if (hide != null) hide.style.display = DisplayStyle.None;
            var showEl = _root.Q<VisualElement>(showPage);
            if (showEl != null) showEl.style.display = DisplayStyle.Flex;
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

            // コラボタブ表示切替
            var colaboTabBtn = _root.Q<Button>("toggle-colabo-tab");
            if (colaboTabBtn != null)
            {
                bool showColabo = _target.showColaboTab;
                colaboTabBtn.text = $"Colabo Tab: {(showColabo ? "ON" : "OFF")}";
                colaboTabBtn.EnableInClassList("part-toggle-off", !showColabo);
                UpdateColaboTabVisibility(showColabo);

                colaboTabBtn.clicked += () =>
                {
                    showColabo = !showColabo;
                    _so.Update();
                    var prop = _so.FindProperty("showColaboTab");
                    if (prop != null)
                    {
                        prop.boolValue = showColabo;
                        _so.ApplyModifiedProperties();
                    }
                    colaboTabBtn.text = $"Colabo Tab: {(showColabo ? "ON" : "OFF")}";
                    colaboTabBtn.EnableInClassList("part-toggle-off", !showColabo);
                    UpdateColaboTabVisibility(showColabo);
                };
            }
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
            // ドロップゾーンで管理するプロパティ — 非表示コンテナに格納
            var hiddenProps = new (string path, string slotName)[]
            {
                ("offTargets", "page0-slot-offTargets"),
                ("headItems", "page0-slot-headItems"),
                ("bodyItems", "page0-slot-bodyItems"),
                ("handItems", "page0-slot-handItems"),
                ("legItems", "page0-slot-legItems"),
            };

            foreach (var (path, slotName) in hiddenProps)
            {
                var slot = _root.Q<VisualElement>(slotName);
                if (slot == null) continue;
                var pf = new PropertyField { bindingPath = path };
                slot.Add(pf);

                // slot自体をUSSで非表示にする（!importantなし）
                // PropertyField追加後にdisplayを設定することで
                // Unity内部の上書きを確実に上回る
                slot.style.display = DisplayStyle.None;
            }

            // 通常のプロパティフィールド
            var hiddenSlotNames = new HashSet<string>(System.Array.ConvertAll(hiddenProps, p => p.slotName));

            for (int pageIdx = 0; pageIdx < PageFieldPaths.Length; pageIdx++)
            {
                foreach (var path in PageFieldPaths[pageIdx])
                {
                    var slotName = $"page{pageIdx}-slot-{path}";
                    if (hiddenSlotNames.Contains(slotName)) continue; // 既に処理済み

                    var slot = _root.Q<VisualElement>(slotName);
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
