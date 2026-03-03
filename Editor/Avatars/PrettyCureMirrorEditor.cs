using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using nadena.dev.modular_avatar.core;

namespace Moruton.Gimmicks.Editor
{
    [CustomEditor(typeof(PrettyCureMirror))]
    public class PrettyCureMirrorEditor : UnityEditor.Editor
    {
        private int selectedLanguage = 0;
        private bool[] stepOpenStates = { true, false, false, false };
        private bool isDeveloperMode = false;
        
        private readonly string[] languageCodes = { "ja", "en" };
        
        private string L(string key)
        {
            return LocalizationManager.Get("PrettyCureMirror", key);
        }
        
        private string LC(string key)
        {
            return LocalizationManager.GetCommon(key);
        }

        private void OnEnable()
        {
            selectedLanguage = EditorPrefs.GetInt("PrettyCureMirror_Language", 0);
            LoadLocalization();
        }
        
        private void LoadLocalization()
        {
            string langCode = languageCodes[selectedLanguage];
            LocalizationManager.Load("PrettyCureMirror", langCode);
        }

        private GUIStyle stepButtonStyle;
        private GUIStyle stepLabelStyle;
        
        public override void OnInspectorGUI()
        {
            if (stepButtonStyle == null)
            {
                stepButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 36,
                    margin = new RectOffset(0, 0, 4, 4),
                    padding = new RectOffset(10, 10, 0, 0)
                };
            }
            
            if (stepLabelStyle == null)
            {
                stepLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    margin = new RectOffset(0, 0, 8, 4)
                };
            }
            
            MorutonAvatarPackageEditorHelper.DrawHeader();
            
            int prevLang = selectedLanguage;
            selectedLanguage = GUILayout.Toolbar(selectedLanguage, LocalizationManager.SupportedLanguageNames, GUILayout.Height(28));
            if (prevLang != selectedLanguage)
            {
                EditorPrefs.SetInt("PrettyCureMirror_Language", selectedLanguage);
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
        
        private void DrawStep1()
        {
            string foldSymbol = stepOpenStates[0] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step1_title"), stepButtonStyle))
            {
                stepOpenStates[0] = !stepOpenStates[0];
            }
            
            if (!stepOpenStates[0]) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step1_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField(L("step1_avatar_label"), stepLabelStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("avatar"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("model"), new GUIContent(L("step1_model")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"), new GUIContent(L("step1_animator")));
            
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(L("step1_before_clothes_label"), stepLabelStyle);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offTargets"), GUIContent.none, true);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawStep2()
        {
            string foldSymbol = stepOpenStates[1] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step2_title"), stepButtonStyle))
            {
                stepOpenStates[1] = !stepOpenStates[1];
            }
            
            if (!stepOpenStates[1]) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step2_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField(L("step2_unpack_title"), stepLabelStyle);
            EditorGUILayout.HelpBox(L("step2_unpack_help"), MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemToUnpack"), GUIContent.none);
            
            EditorGUILayout.Space(4);
            if (GUILayout.Button(L("step2_unpack_button"), GUILayout.Height(28)))
            {
                UnpackSelectedPrefab();
            }
            
            EditorGUILayout.Space(12);
            
            EditorGUILayout.LabelField(L("step2_parts_title"), stepLabelStyle);
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
                        {
                            GUILayout.Box(preview, GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                        }
                        else
                        {
                            GUILayout.Box("", GUILayout.Width(previewSize), GUILayout.Height(previewSize));
                        }
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
        
        private void DrawStep3()
        {
            string foldSymbol = stepOpenStates[2] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step3_title"), stepButtonStyle))
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
        
        private void DrawStep4()
        {
            string foldSymbol = stepOpenStates[3] ? "▼ " : "▶ ";
            if (GUILayout.Button(foldSymbol + L("step4_title"), stepButtonStyle))
            {
                stepOpenStates[3] = !stepOpenStates[3];
            }
            
            if (!stepOpenStates[3]) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L("step4_description"), EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(8);
            
            var script = (PrettyCureMirror)target;
            
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
            
            EditorGUILayout.LabelField(L("step4_onepiece_title"), stepLabelStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(L("step4_onepiece_label"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onePiece"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("FBX");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboFBX"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField(L("step4_additional_item_title"), stepLabelStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(L("step4_additional_item_target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboItemTarget"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(L("step4_additional_item"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboItem"), GUIContent.none);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
            
            EditorGUILayout.LabelField(L("step4_fade_fx_title"), stepLabelStyle);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Head", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeHead"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeHeadItems"), GUIContent.none, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeHeadMaterial"), GUIContent.none);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Body", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeBody"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeBodyItems"), GUIContent.none, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeBodyMaterial"), GUIContent.none);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Arm", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeArm"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeArmItems"), GUIContent.none, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeArmMaterial"), GUIContent.none);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Leg", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeLeg"), GUIContent.none);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeLegItems"), GUIContent.none, true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeLegMaterial"), GUIContent.none);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDeveloperMode()
        {
            string foldSymbol = isDeveloperMode ? "▼ " : "▶ ";
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.6f, 0.6f, 0.7f);
            
            if (GUILayout.Button(foldSymbol + LC("dev_mode_title"), stepButtonStyle))
            {
                isDeveloperMode = !isDeveloperMode;
            }
            GUI.backgroundColor = prevColor;
            
            if (!isDeveloperMode) return;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField(LC("dev_basic_label"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dummyImage"), new GUIContent(L("dev_dummy_image")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboShopTex"), new GUIContent(L("dev_colabo_shop_tex")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboShopInfo"), new GUIContent(L("dev_colabo_shop_info")));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("model"), new GUIContent("Model"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"), new GUIContent("Animator"));
            
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(LC("dev_target_label"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headTarget"), new GUIContent("Head Target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyTarget"), new GUIContent("Body Target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("handTarget"), new GUIContent("Hand Target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("legTarget"), new GUIContent("Leg Target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onePiece"), new GUIContent("OnePiece"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colaboItemTarget"), new GUIContent("Colabo Item Target"));
            
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(LC("dev_material_label"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeHeadMaterial"), new GUIContent("Head Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeBodyMaterial"), new GUIContent("Body Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeArmMaterial"), new GUIContent("Arm Material"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeLegMaterial"), new GUIContent("Leg Material"));
            
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(LC("dev_gimmick_color_targets"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gimmickCollar"), GUIContent.none, true);
            
            EditorGUILayout.EndVertical();
        }
        
        private void ApplyGimmickColor()
        {
            var script = (PrettyCureMirror)target;
            var gimmickCollar = script.GimmickCollar;
            if (gimmickCollar == null) return;
            
            int changed = 0;
            foreach (var go in gimmickCollar)
            {
                if (go == null) continue;
                var systems = go.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var ps in systems)
                {
                    var main = ps.main;
                    main.startColor = script.gimmickColor;
                    changed++;
                }
            }
            
            if (changed > 0)
            {
                Debug.Log($"Applied color to {changed} particle systems.");
            }
        }
        
        private void UnpackSelectedPrefab()
        {
            var script = (PrettyCureMirror)target;
            var item = script.itemToUnpack;
            
            if (item == null)
            {
                EditorUtility.DisplayDialog(LC("unpack_info_title"), LC("unpack_dialog_info"), LC("ok"));
                return;
            }
            
            if (PrefabUtility.IsPartOfPrefabInstance(item))
            {
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(item);
                if (root != null)
                {
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    string msg = string.Format(LC("unpack_success"), root.name);
                    EditorUtility.DisplayDialog(LC("unpack_success_title"), msg, LC("ok"));
                }
            }
            else
            {
                string msg = string.Format(LC("unpack_not_prefab"), item.name);
                EditorUtility.DisplayDialog(LC("unpack_info_title"), msg, LC("ok"));
            }
        }
        
        private void SetupTransformation()
        {
            var script = (PrettyCureMirror)target;
            
            if (script.Model == null || script.Animator == null || script.OffTargets == null || script.OffTargets.Length == 0)
            {
                EditorUtility.DisplayDialog(LC("error_dialog_title"), LC("error_dialog_message"), LC("ok"));
                return;
            }
            
            // ワンピース差し替え
            if (script.ColaboFBX != null && script.OnePiece != null)
            {
                ReplaceOnePieceChild(script.OnePiece.transform, script.ColaboFBX);
            }
            
            // アイテム装着
            ProcessItemAttachment(script.headTarget, script.headItems);
            ProcessItemAttachment(script.bodyTarget, script.bodyItems);
            ProcessItemAttachment(script.handTarget, script.handItems);
            ProcessItemAttachment(script.legTarget, script.legItems);
            
            // 追加アイテム
            if (script.colaboItemTarget != null && script.colaboItem != null)
            {
                ReplaceOnePieceChild(script.colaboItemTarget, script.colaboItem);
            }
            
            // フェード演出
            if (script.fadeHead != null)
                ProcessFadeAttachment(script.fadeHead, script.fadeHeadItems, script.fadeHeadMaterial);
            if (script.fadeBody != null)
                ProcessFadeAttachment(script.fadeBody, script.fadeBodyItems, script.fadeBodyMaterial);
            if (script.fadeArm != null)
                ProcessFadeAttachment(script.fadeArm, script.fadeArmItems, script.fadeArmMaterial);
            if (script.fadeLeg != null)
                ProcessFadeAttachment(script.fadeLeg, script.fadeLegItems, script.fadeLegMaterial);
            
            // アニメーション生成
            CreateAnimations(script);
        }
        
        private void ReplaceOnePieceChild(Transform parent, GameObject newItem)
        {
            if (parent == null || newItem == null) return;
            
            // 既存の子を削除（名前が一致しないもの）
            var toDelete = new List<Transform>();
            foreach (Transform child in parent)
            {
                if (child.name != newItem.name)
                    toDelete.Add(child);
            }
            foreach (var child in toDelete)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
            
            // 新しいアイテムを追加
            bool exists = false;
            foreach (Transform child in parent)
            {
                if (child.name == newItem.name)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                GameObject instance;
                if (PrefabUtility.IsPartOfPrefabAsset(newItem))
                {
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(newItem, parent);
                    Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
                }
                else
                {
                    instance = newItem;
                    Undo.SetTransformParent(instance.transform, parent, "Move Item");
                }
                
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = newItem.name;
            }
        }
        
        private void ProcessItemAttachment(Transform target, GameObject[] items)
        {
            if (target == null) return;
            if (items == null) items = new GameObject[0];
            
            var itemSet = new HashSet<GameObject>(items);
            
            // 不要な子を削除
            var toDelete = new List<Transform>();
            foreach (Transform child in target)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (source != null && itemSet.Contains(source)) continue;
                if (itemSet.Contains(child.gameObject)) continue;
                toDelete.Add(child);
            }
            foreach (var child in toDelete)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
            
            // 新しいアイテムを追加
            foreach (var item in items)
            {
                if (item == null) continue;
                
                bool exists = false;
                foreach (Transform child in target)
                {
                    var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (source == item || child.gameObject == item)
                    {
                        exists = true;
                        break;
                    }
                }
                
                if (!exists)
                {
                    GameObject instance;
                    if (PrefabUtility.IsPartOfPrefabAsset(item))
                    {
                        instance = (GameObject)PrefabUtility.InstantiatePrefab(item, target);
                        Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
                    }
                    else
                    {
                        instance = item;
                        Undo.SetTransformParent(instance.transform, target, "Move Item");
                    }
                    
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.name = item.name;
                }
            }
        }
        
        private void ProcessFadeAttachment(Transform target, GameObject[] items, Material material)
        {
            if (target == null) return;
            
            // 既存の子を全削除
            var toDelete = new List<Transform>();
            foreach (Transform child in target)
            {
                toDelete.Add(child);
            }
            foreach (var child in toDelete)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
            
            if (items == null) return;
            
            foreach (var item in items)
            {
                if (item == null) continue;
                
                GameObject instance;
                if (PrefabUtility.IsPartOfPrefabAsset(item))
                {
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(item, target);
                }
                else
                {
                    instance = Instantiate(item, target);
                }
                Undo.RegisterCreatedObjectUndo(instance, "Create Fade Item");
                
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = target.name;
                
                // マテリアル差し替え
                if (material != null)
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        var mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++)
                        {
                            mats[i] = material;
                        }
                        r.sharedMaterials = mats;
                    }
                }
            }
        }
        
        private void CreateAnimations(PrettyCureMirror script)
        {
            string scriptAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(script);
            string basePath;
            
            if (!string.IsNullOrEmpty(scriptAssetPath))
            {
                basePath = Path.GetDirectoryName(scriptAssetPath);
            }
            else
            {
                basePath = "Assets/Morulab/PrettyCureMirror";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
            }
            
            string animFolder = Path.Combine(basePath, "Animation");
            if (!Directory.Exists(animFolder))
            {
                Directory.CreateDirectory(animFolder);
            }
            
            string enablePath = Path.Combine(animFolder, "Enable.anim");
            string disablePath = Path.Combine(animFolder, "Disable.anim");
            
            // Enable Animation
            AnimationClip enableClip = new AnimationClip();
            foreach (var obj in script.OffTargets)
            {
                if (obj == null) continue;
                string path = GetPath(script.Avatar, obj);
                enableClip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 0));
            }
            
            string modelPath = GetPath(script.Avatar, script.Model);
            enableClip.SetCurve(modelPath, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 1));
            
            AssetDatabase.CreateAsset(enableClip, enablePath);
            
            // Disable Animation
            AnimationClip disableClip = new AnimationClip();
            foreach (var obj in script.OffTargets)
            {
                if (obj == null) continue;
                string path = GetPath(script.Avatar, obj);
                disableClip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 1));
            }
            
            disableClip.SetCurve(modelPath, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 0));
            
            AssetDatabase.CreateAsset(disableClip, disablePath);
            
            AssetDatabase.SaveAssets();
            
            // AnimatorControllerに適用
            var controller = script.Animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                SetAnimationToState(controller, "Enable", enableClip);
                SetAnimationToState(controller, "Disable", disableClip);
                Debug.Log("Animations applied successfully.");
            }
            else
            {
                EditorUtility.DisplayDialog(LC("error_dialog_title"), "AnimatorController not set.", LC("ok"));
            }
        }
        
        private string GetPath(GameObject root, GameObject child)
        {
            var path = new List<string>();
            var current = child.transform;
            while (current != null && current != root.transform)
            {
                path.Add(current.name);
                current = current.parent;
            }
            path.Reverse();
            return string.Join("/", path);
        }
        
        private void SetAnimationToState(AnimatorController controller, string stateName, AnimationClip clip)
        {
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                    {
                        state.state.motion = clip;
                        return;
                    }
                }
            }
            
            var newState = controller.layers[0].stateMachine.AddState(stateName);
            newState.motion = clip;
        }
    }
}
