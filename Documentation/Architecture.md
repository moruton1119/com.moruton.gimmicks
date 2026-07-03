# com.moruton.gimmicks アーキテクチャ

> 最終更新: beta.160

## 全体構成

```
com.moruton.gimmicks/
├── Runtime/
│   ├── com.moruton.gimmicks.asmdef     # MA参照（#if MODULAR_AVATARでガード）
│   ├── MorutonGimmickPackage.cs        # 基底クラス
│   ├── Avatars/
│   │   ├── MorutonAvatarPackage.cs     # アバター専用基底
│   │   ├── Metamorphose.cs             # 変身ギミック本体（データ保持）
│   │   ├── GimmickSetupHelper.cs       # セットアップ補助
│   │   ├── Item_Randomiser.cs          # アイテムランダマイザー
│   │   └── ItemSetupScript.cs          # アイテムセットアップ
│   └── Common/
│       ├── SetupTarget.cs              # 汎用データ構造
│       └── ItemCopyUtility.cs          # コピー処理
│
├── Editor/
│   ├── com.moruton.gimmicks.Editor.asmdef
│   ├── Avatars/
│   │   ├── LocalizationManager.cs      # 多言語管理
│   │   ├── MorutonAvatarPackageEditorHelper.cs  # 共通ヘッダー・更新通知
│   │   ├── Metamorphose/               # 変身ギミックUI・ビルド
│   │   │   ├── MetamorphoseEditor.cs              # Inspector
│   │   │   ├── MetamorphosePlugin.cs               # NDMF Plugin登録
│   │   │   ├── MetamorphoseApplyPass.cs            # NDMF Build Pass
│   │   │   ├── MetamorphoseSetupService.cs         # エディタユーティリティ
│   │   │   ├── ProtectedAnimLoader.cs              # 暗号化DLL読み込み
│   │   │   ├── ProtectedAnimClipBuilder.cs         # バイナリ→AnimationClip復元
│   │   │   ├── EditorThemeDefinition.cs            # テーマ色定義
│   │   │   ├── EditorThemeRegistry.cs              # テーマ登録管理
│   │   │   ├── MagicalOpeningEffect.cs             # OP演出
│   │   │   ├── MetamorphoseWindow.cs               # セットアップウィンドウ
│   │   │   ├── MetamorphoseWindow.Theme.cs         # テーマ制御
│   │   │   ├── MetamorphoseWindow.Preview.cs       # プレビュー描画
│   │   │   ├── MetamorphoseWindow.Navigation.cs    # ページ遷移・D&D
│   │   │   ├── MetamorphoseWindow.Localization.cs  # 多言語適用
│   │   │   ├── MetamorphoseWindow.Banner.cs        # バナー広告
│   │   │   ├── MetamorphoseWindow.Buttons.cs       # ボタンコールバック
│   │   │   ├── MetamorphoseWindow.uxml             # UI定義
│   │   │   ├── MetamorphoseWindow.uss              # 共通スタイル
│   │   │   ├── Theme_Moonlight.uss                 # テーマ: 月光（ダーク）
│   │   │   ├── Theme_Daylight.uss                  # テーマ: 昼光（ライト）
│   │   │   ├── Theme_Cyber.uss                     # テーマ: サイバー
│   │   │   ├── Theme_Wizard.uss                    # テーマ: 魔法使い
│   │   │   └── Theme_Diamond.uss                   # テーマ: ダイヤモンド
│   │   ├── GimmickSetupHelperEditor.cs
│   │   ├── Item_RandomiserEditor.cs
│   │   └── ItemSetupScriptEditor.cs
│   ├── Common/
│   │   ├── AnimationBuilder.cs         # AnimationClip生成
│   │   ├── GimmickPrefabUtility.cs     # Prefab操作
│   │   ├── ItemPlacer.cs               # アイテム配置
│   │   └── EditorStyleFactory.cs       # GUIStyle（※現在未使用）
│   └── Core/UpdateChecker/             # 自動アップデート（独立asmdef）
│       ├── SemVer.cs                   # SemVer 2.0 パーサー
│       └── GimmicksUpdateChecker.cs    # バージョンチェック・自動更新
│
└── Documentation/
```

## 変身ギミック（Metamorphose）のビルドフロー

### NDMFビルドパイプライン

```
Resolving Phase
  ├─ MetamorphoseApplyPass（BeforePlugin: MA）
  │   ├─ GenerateAnimations
  │   │   └─ Enable/Disable AnimationClipをControllerのStateに設定
  │   ├─ InjectProtectedAnimations
  │   │   ├─ ProtectedAnimLoader.LoadDll() — DLL読み込み
  │   │   ├─ ProtectedAnimLoader.LoadDecrypted() — 復号
  │   │   ├─ ProtectedAnimClipBuilder.Build() — バイナリ→AnimationClip
  │   │   └─ AnimationBuilder.ApplyClipToState() — Stateに設定
  │   ├─ ItemPlacer.PlaceItems() — 衣装をアバターに配置
  │   └─ UnpackAllPrefabs() — Prefabを完全展開
  │
  ├─ Modular Avatar
  │   ├─ Clone animators — ControllerをClone（上記Clip含む）
  │   └─ MergeAnimator — 統合
  │
Transforming Phase
  └─ Modular Avatar
      ├─ MergeArmature
      ├─ MenuInstall
      └─ その他統合処理
```

### 条件付きコンパイル

| シンボル | 定義条件 | 影響 |
|---|---|---|
| `MODULAR_AVATAR` | `nadena.dev.modular-avatar`インストール時 | MA連携機能が有効化 |
| `VRC_SDK_AVATARS` | `com.vrchat.avatars`インストール時 | VRC SDK機能が有効化 |

MAがない環境では：
- `Metamorphose` は `MonoBehaviour` として動作（`AvatarTagComponent`の代わり）
- NDMF Plugin / ApplyPass はコンパイルされない
- エラーなしで読み込まれる

## テーマシステム

色は全て `EditorThemeDefinition` 構造体 + USS（CSS変数）で管理。
**C#でのインライン色指定は禁止**（詳細: [AntiHardcodeRules.md](AntiHardcodeRules.md)）。

5テーマ: Moonlight / Daylight / Cyber / Wizard / Diamond

## ProtectedAnimationSystem

暗号化されたアニメーションDLLから、ビルド時に復元して注入するシステム。

```
EncryptedAnimData.dll
  ├─ AES-256で暗号化されたアニメーションバイナリ
  ├─ AES鍵（DLL内に埋め込み）
  └─ GetDecryptedData(key) メソッド
```

ビルド時に `ProtectedAnimLoader` がDLLを読み込み、`ProtectedAnimClipBuilder` がバイナリからAnimationClipを復元する。

詳細: [ProtectedAnimationSystem_Design.md](ProtectedAnimationSystem.md)
