using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミック共通のInspector Editor。
    /// PrettyCureMirror等の変身スクリプトで使用。
    /// </summary>
    [CustomEditor(typeof(PrettyCureMirror))]
    public class MetamorphoseEditor : UnityEditor.Editor
    {
        private int selectedLanguage = 0;
        private bool[] stepOpenStates = { true, false, false, false };
        private bool isDeveloperMode = false;

        private readonly string[] languageCodes = { "ja", "en", "ko", "it", "es" };

        private string L(string key) => LocalizationManager.Get("PrettyCureMirror", key);
        private string LC(string key) => LocalizationManager.GetCommon(key);

        private void OnEnable()
        {
            selectedLanguage = EditorPrefs.GetInt("MetamorphoseEditor_Language", 0);
            LoadLocalization();
        }

        private void LoadLocalization()
        {
            LocalizationManager.Load("PrettyCureMirror", languageCodes[selectedLanguage]);
        }

        public override void OnInspectorGUI()
        {
            MorutonAvatarPackageEditorHelper.DrawHeader();

            // 言語切替
            int prevLang = selectedLanguage;
            selectedLanguage = GUILayout.Toolbar(selectedLanguage, LocalizationManager.SupportedLanguageNames, GUILayout.Height(28));
            if (prevLang != selectedLanguage)
            {
                EditorPrefs.SetInt("MetamorphoseEditor_Language", selectedLanguage);
                LoadLocalization();
            }

            serializedObject.Update();

            EditorGUILayout.Space(8);
            DrawStep1();
            EditorGUILayout.Space(4);
            DrawStep2();
            EditorGUILayout.Space(4);
            DrawStep3();
            EditorGUILayout.Space(4);
            DrawStep4();
            EditorGUILayout.Space(8);
            DrawDeveloperMode();

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button(LC("setup_button"), GUILayout.Height(44)))
            {
                SetupTransformation();
            }
            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }

        #region Step 1: 基本設定

        private void DrawStep1()
        {
            string foldSymbol = stepOpenStates[0] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step1_title"), EditorStyleFactory.StepButtonStyle))
            {
                stepOpenStates[0] = !stepOpenStates[0];
            }

            if (!stepOpenStates[0]) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step1_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField(L("step1_avatar_label"), EditorStyleFactory.StepLabelStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("avatar"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("model"), new GUIContent(L("step1_model")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"), new GUIContent(L("step1_animator")));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(L("step1_before_clothes_label"), EditorStyleFactory.StepLabelStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offTargets"), GUIContent.none, true);

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Step 2: 変身後の衣装

        private void DrawStep2()
        {
            string foldSymbol = stepOpenStates[1] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step2_title"), EditorStyleFactory.StepButtonStyle))
            {
                stepOpenStates[1] = !stepOpenStates[1];
            }

            if (!stepOpenStates[1]) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step2_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField(L("step2_unpack_title"), EditorStyleFactory.StepLabelStyle);
            EditorGUILayout.HelpBox(L("step2_unpack_help"), MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemToUnpack"), GUIContent.none);

            EditorGUILayout.Space(4);
            if (GUILayout.Button(L("step2_unpack_button"), GUILayout.Height(28)))
            {
                var itemToUnpack = serializedObject.FindProperty("itemToUnpack").objectReferenceValue as GameObject;
                GimmickPrefabUtility.UnpackPrefab(itemToUnpack);
            }

            EditorGUILayout.Space(12);

            EditorGUILayout.LabelField(L("step2_parts_title"), EditorStyleFactory.StepLabelStyle);
            EditorGUILayout.HelpBox(L("step2_parts_help"), MessageType.Info);

            EditorGUILayout.Space(4);

            var script = (PrettyCureMirror)target;

            DrawPartSection(L("step2_head_target"), L("step2_head_items"),
                serializedObject.FindProperty("headTarget"), serializedObject.FindProperty("headItems"),
                script.headItems);

            DrawPartSection(L("step2_body_target"), L("step2_body_items"),
                serializedObject.FindProperty("bodyTarget"), serializedObject.FindProperty("bodyItems"),
                script.bodyItems);

            DrawPartSection(L("step2_hand_target"), L("step2_hand_items"),
                serializedObject.FindProperty("handTarget"), serializedObject.FindProperty("handItems"),
                script.handItems);

            DrawPartSection(L("step2_leg_target"), L("step2_leg_items"),
                serializedObject.FindProperty("legTarget"), serializedObject.FindProperty("legItems"),
                script.legItems);

            EditorGUILayout.EndVertical();
        }

        private void DrawPartSection(string targetLabel, string itemsLabel, SerializedProperty targetProp, SerializedProperty itemsProp, GameObject[] items)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(targetLabel);
            EditorGUILayout.PropertyField(targetProp, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(itemsProp, new GUIContent(itemsLabel), true);

            if (items != null && items.Length > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                int previewSize = 64;
                int maxShow = Mathf.Min(items.Length, 4);
                int totalWidth = maxShow * previewSize + (maxShow - 1) * 4;

                EditorGUILayout.BeginHorizontal(GUILayout.Width(totalWidth));
                for (int i = 0; i < maxShow; i++)
                {
                    if (items[i] != null)
                    {
                        Texture2D preview = AssetPreview.GetAssetPreview(items[i]);
                        if (preview != null)
                            GUILayout.Box(preview, GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                        else
                            GUILayout.Box("", GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (items.Length > maxShow)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField($"+{items.Length - maxShow} more...", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        #endregion

        #region Step 3: ギミック色

        private void DrawStep3()
        {
            string foldSymbol = stepOpenStates[2] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step3_title"), EditorStyleFactory.StepButtonStyle))
            {
                stepOpenStates[2] = !stepOpenStates[2];
            }

            if (!stepOpenStates[2]) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step3_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gimmickColor"), new GUIContent(L("step3_color_label")));
            if (EditorGUI.EndChangeCheck())
            {
                ApplyGimmickColor();
            }

            EditorGUILayout.EndVertical();
        }

        private void ApplyGimmickColor()
        {
            var script = (PrettyCureMirror)target;
            if (script.GimmickCollar == null) return;

            foreach (var collar in script.GimmickCollar)
            {
                if (collar == null) continue;

                var renderers = collar.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat == null || !mat.HasProperty("_Color")) continue;
                        mat.color = script.gimmickColor;
                    }
                }
            }
        }

        #endregion

        #region Step 4: コラボ・フェード演出

        private void DrawStep4()
        {
            string foldSymbol = stepOpenStates[3] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step4_title"), EditorStyleFactory.StepButtonStyle))
            {
                stepOpenStates[3] = !stepOpenStates[3];
            }

            if (!stepOpenStates[3]) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step4_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);

            var script = (PrettyCureMirror)target;

            // コラボ情報
            if (script.colaboShopTex != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(script.colaboShopTex, GUILayout.Width(120), GUILayout.Height(120));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(script.colaboShopInfo))
                {
                    if (GUILayout.Button(LC("colabo_shop_button"), GUILayout.Height(32)))
                    {
                        Application.OpenURL(script.colaboShopInfo);
                    }
                }
                EditorGUILayout.Space(8);
            }

            EditorGUILayout.HelpBox(LC("colabo_info"), MessageType.Info);
            EditorGUILayout.Space(8);

            // ワンピース
            EditorGUILayout.LabelField(L("step4_onepiece_title"), EditorStyleFactory.StepLabelStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(L("step4_onepiece_label"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onePiece"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("FBX");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboFBX"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            // 追加アイテム
            EditorGUILayout.LabelField(L("step4_additional_item_title"), EditorStyleFactory.StepLabelStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(L("step4_additional_item_target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboItemTarget"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(L("step4_additional_item"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboItem"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);

            // フェード演出
            EditorGUILayout.LabelField(L("step4_fade_fx_title"), EditorStyleFactory.StepLabelStyle);
            DrawFadeSection("Head", "fadeHead", "fadeHeadItems", "fadeHeadMaterial");
            DrawFadeSection("Body", "fadeBody", "fadeBodyItems", "fadeBodyMaterial");
            DrawFadeSection("Arm", "fadeArm", "fadeArmItems", "fadeArmMaterial");
            DrawFadeSection("Leg", "fadeLeg", "fadeLegItems", "fadeLegMaterial");

            EditorGUILayout.EndVertical();
        }

        private void DrawFadeSection(string label, string transformProp, string itemsProp, string materialProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(transformProp), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(itemsProp), GUIContent.none, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(materialProp), GUIContent.none);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        #endregion

        #region Developer Mode

        private void DrawDeveloperMode()
        {
            isDeveloperMode = EditorGUILayout.Foldout(isDeveloperMode, "Developer Mode");
            if (!isDeveloperMode) return;

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Generate Animations", GUILayout.Width(150));
            if (GUILayout.Button("Generate"))
            {
                GenerateAnimations();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Re-process Setup", EditorStyles.miniLabel);
            EditorGUILayout.Space(2);
            if (GUILayout.Button("Full Re-process"))
            {
                SetupTransformation();
            }
        }

        #endregion

        #region Setup Execution

        private void SetupTransformation()
        {
            var script = (PrettyCureMirror)target;

            // アイテム装着
            ProcessItemAttachment(script);

            // ワンピース差し替え
            if (script.OnePiece != null && script.ColaboFBX != null)
            {
                ReplaceOnePieceChild(script.OnePiece, script.ColaboFBX);
            }

            // コラボアイテム
            if (script.colaboItemTarget != null && script.colaboItem != null)
            {
                var instance = Instantiate(script.colaboItem, script.colaboItemTarget);
                instance.name = script.colaboItem.name;
            }

            // フェード装着
            ProcessFadeAttachment(script);

            // アニメーション生成
            GenerateAnimations();

            // MA Merge Animator 生成
            GenerateMergeAnimator(script);

            Debug.Log("[MetamorphoseEditor] Setup complete!");
        }

        private void ProcessItemAttachment(PrettyCureMirror script)
        {
            ProcessPart(script.headTarget, script.headItems);
            ProcessPart(script.bodyTarget, script.bodyItems);
            ProcessPart(script.handTarget, script.handItems);
            ProcessPart(script.legTarget, script.legItems);
        }

        private void ProcessPart(Transform target, GameObject[] items)
        {
            if (target == null || items == null) return;

            while (target.childCount > 0)
                DestroyImmediate(target.GetChild(0).gameObject);

            foreach (var item in items)
            {
                if (item == null) continue;
                var instance = Instantiate(item, target);
                instance.name = item.name;
            }
        }

        private void ReplaceOnePieceChild(GameObject onePiece, GameObject fbx)
        {
            if (onePiece == null || fbx == null) return;

            while (onePiece.transform.childCount > 0)
                DestroyImmediate(onePiece.transform.GetChild(0).gameObject);

            var instance = Instantiate(fbx, onePiece.transform);
            instance.name = fbx.name;
        }

        private void ProcessFadeAttachment(PrettyCureMirror script)
        {
            ProcessFadePart(script.fadeHead, script.fadeHeadItems, script.fadeHeadMaterial);
            ProcessFadePart(script.fadeBody, script.fadeBodyItems, script.fadeBodyMaterial);
            ProcessFadePart(script.fadeArm, script.fadeArmItems, script.fadeArmMaterial);
            ProcessFadePart(script.fadeLeg, script.fadeLegItems, script.fadeLegMaterial);
        }

        private void ProcessFadePart(Transform target, GameObject[] items, Material fadeMaterial)
        {
            if (target == null || items == null) return;

            while (target.childCount > 0)
                DestroyImmediate(target.GetChild(0).gameObject);

            foreach (var item in items)
            {
                if (item == null) continue;
                var instance = Instantiate(item, target);
                instance.name = item.name;

                if (fadeMaterial != null)
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        var materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++)
                            materials[i] = fadeMaterial;
                        renderer.sharedMaterials = materials;
                    }
                }
            }
        }

        private void GenerateAnimations()
        {
            var script = (PrettyCureMirror)target;
            if (script.Avatar == null || script.Animator == null) return;

            string basePath = GetBasePath(script);
            string animFolder = Path.Combine(basePath, "Animation");

            var (enableClip, disableClip) = AnimationBuilder.CreateToggleAnimations(
                script.Avatar, script.OffTargets, script.Model, animFolder, "Enable", "Disable");

            if (enableClip == null || disableClip == null) return;

            var controller = script.Animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                AnimationBuilder.ApplyClipToState(controller, "Enable", enableClip);
                AnimationBuilder.ApplyClipToState(controller, "Disable", disableClip);
                Debug.Log("[MetamorphoseEditor] Animations applied successfully.");
            }
            else
            {
                EditorUtility.DisplayDialog(LC("error_dialog_title"), "AnimatorController not set.", LC("ok"));
            }
        }

        private void GenerateMergeAnimator(PrettyCureMirror script)
        {
            if (script.Avatar == null) return;

            var mergeAnimator = script.Avatar.AddComponent<MergeAnimator>();
            mergeAnimator.animator = script.Animator;

            // MA PathMap の設定（裏地衣装切替）
            if (script.OnePiece != null && script.Model != null)
            {
                var offPaths = new List<string>();
                foreach (var obj in script.OffTargets)
                {
                    if (obj == null) continue;
                    string path = AnimationBuilder.GetRelativePath(script.Avatar, obj);
                    if (!string.IsNullOrEmpty(path))
                        offPaths.Add(path);
                }

                if (offPaths.Count > 0)
                {
                    var pathMap = new MergeBlendShapePathMap();
                    pathMap.Object = script.Model;
                    pathMap.BlendShapeName = "nul";

                    mergeAnimator.PathMap.AddItem(pathMap);
                }
            }

            EditorUtility.SetDirty(script.Avatar);
        }

        private string GetBasePath(MonoBehaviour script)
        {
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(script);
            if (!string.IsNullOrEmpty(assetPath))
                return Path.GetDirectoryName(assetPath);

            string fallback = "Assets/Morulab/PrettyCureMirror";
            if (!Directory.Exists(fallback))
                Directory.CreateDirectory(fallback);
            return fallback;
        }

        #endregion
    }
}
