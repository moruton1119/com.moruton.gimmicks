# com.moruton.gimmicks Script Index

## 📦 パッケージ概要

| 項目 | 値 |
|---|---|
| パッケージ名 | `com.moruton.gimmicks` |
| バージョン | v0.3.0-beta.2 |
| Unity | 2022.3 |
| 対応 | アバター / ワールド（共通部分はMA非依存） |
| 依存 | Modular Avatar（条件付き）, VRChat SDK Base（条件付き） |

## 📁 ファイル構成

```
com.moruton.gimmicks/
├── package.json
├── .github/workflows/
│   └── release.yml                    # VPMパッケージ自動リリース（tag push → gh-pagesデプロイ）
├── Runtime/
│   ├── com.moruton.gimmicks.asmdef
│   ├── MorutonGimmickPackage.cs       # 汎用基底クラス（現状未使用）
│   ├── Common/                        # 🔧 共通クラス（MA/VRC SDK非依存）
│   │   ├── SetupTarget.cs             # 汎用データ構造
│   │   └── ItemCopyUtility.cs         # GameObjectコピー処理
│   ├── Avatars/                       # 🎭 アバター用コンポーネント
│   │   ├── MorutonAvatarPackage.cs    # アバター基底クラス
│   │   ├── GimmickSetupHelper.cs      # セットアップ補助
│   │   ├── Metamorphose.cs            # 変身ギミック（データ保持）
│   │   ├── Item_Randomiser.cs         # アイテムランダマイザー
│   │   └── ItemSetupScript.cs         # アイテムセットアップ
│   ├── Shaders/
│   │   └── CommonParticle.shader      # パーティクル用シェーダー（CommonParticle.shader にリネーム予定）
│   └── Common/
│       └── Morulabw.png               # ロゴ画像
├── Editor/
│   ├── com.moruton.gimmicks.Editor.asmdef
│   ├── Core/UpdateChecker/             # 🔄 自動アップデート（独立asmdef）
│   │   ├── MorutonGimmicks.UpdateChecker.Editor.asmdef
│   │   ├── SemVer.cs                   # SemVer 2.0 パーサー
│   │   └── GimmicksUpdateChecker.cs   # バージョンチェック・自動更新
│   ├── Common/                         # 🔧 Editor共通ユーティリティ
│   │   ├── EditorStyleFactory.cs       # GUIStyle共通生成
│   │   ├── GimmickPrefabUtility.cs     # Prefab操作ユーティリティ
│   │   ├── AnimationBuilder.cs         # アニメーション生成ユーティリティ
│   │   ├── TargetListDrawer.cs         # ターゲットリストUI描画
│   │   └── ItemListDrawer.cs           # アイテムリストUI描画
│   ├── Avatars/                        # 🎭 アバター用Editor
│   │   ├── MorutonAvatarPackageEditorHelper.cs  # 共通ヘッダー・アップデートUI
│   │   ├── MetamorphoseEditor.cs       # 変身ギミックEditor（Metamorphose用）
│   │   ├── GimmickSetupHelperEditor.cs # GimmickSetupHelper用Inspector
│   │   ├── Item_RandomiserEditor.cs    # Item_Randomiser用Inspector
│   │   ├── ItemSetupScriptEditor.cs   # ItemSetupScript用Inspector
│   │   ├── LocalizationManager.cs      # 多言語管理（5言語対応）
│   │   └── Localization/
│   │       ├── Common/{ja,en,ko,it,es}.json
│   │       └── Metamorphose/{ja,en,ko,it,es}.json
│   └── Worlds/                         # 🌍 ワールド用Editor（今後拡張）
└── Documentation/
    ├── Architecture.md
    └── ReleaseWorkflow.md
```

---

## Runtime Layer

### 🏗️ 基底クラス

| スクリプト | 名前空間 | 継承元 | 役割 |
|---|---|---|---|
| `MorutonGimmickPackage.cs` | `Moruton.Gimmicks` | `AvatarTagComponent` / `MonoBehaviour` | ギミック汎用基底クラス。MA有無で条件付き継承。**現状未使用。** |
| `MorutonAvatarPackage.cs` | `Moruton.Gimmicks` | `AvatarTagComponent` / `MonoBehaviour` | アバター用ギミック基底クラス。MA有無で条件付き継承。`GimmickSetupHelper` / `Item_Randomiser` の親。 |

> **条件付き継承の仕組み**: `com.moruton.gimmicks.asmdef` の `versionDefines` で `MODULAR_AVATAR` / `VRC_SDK_AVATARS` シンボルを定義。MAがインストールされていれば `AvatarTagComponent` を継承、なければ `MonoBehaviour` にフォールバック。

---

### 🔧 Runtime/Common（共通クラス）

#### `SetupTarget.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | セットアップ対象の汎用データ構造。複数スクリプトで共用。 |
| 含まれる型 | `SetupTarget` (description + targetObject), `ItemData` (sourceObject + targetParent) |
| 使用箇所 | `GimmickSetupHelper`, `Item_Randomiser`, `ItemSetupScript`, `GimmickPrefabUtility` |

#### `ItemCopyUtility.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | GameObjectのコピー処理。アイテムをターゲット親の下にインスタンス化。 |
| 主要メソッド | `CopyAllToTarget(List<ItemData>)` — 既存の子を全削除してからコピー |
| 使用箇所 | `Item_Randomiser.CopyAllToTarget()`, `ItemSetupScript.CopyAllToTarget()` |

---

### 🎭 Runtime/Avatars（アバター用コンポーネント）

#### `Metamorphose.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | プリキュア変身ギミックの**データ保持専用クラス**。Inspector上の設定値を格納。 |
| 継承 | `AvatarTagComponent`（MA必須） |
| 編集属性 | `[ExecuteInEditMode]` — Editor上で動作 |
| 持っているデータ | avatar, model, animator, offTargets, head/body/hand/legの衣装アイテム, ワンピース設定, コラボアイテム, フェード演出4部位, ギミック色 |
| 実行時処理 | **なし** — セットアップ・アニメーション生成は `MetamorphoseEditor` 側で行う |
| 対応Editor | `MetamorphoseEditor` |

#### `GimmickSetupHelper.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | アバターのセットアップ補助。ターゲットオブジェクトを説明文付きでリスト管理。 |
| 継承 | `MorutonAvatarPackage` |
| 持っているデータ | dummyImage, `List<SetupTarget> targets` |
| 使用用途 | Inspector上で「このターゲットを選択」ボタンを表示し、シーン内オブジェクトに飛べるようにする |
| 対応Editor | `GimmickSetupHelperEditor` |

#### `Item_Randomiser.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | アイテムの位置調整＆入れ替え。2つのタブ機能を持つ。 |
| 継承 | `MorutonAvatarPackage` |
| 持っているデータ | `List<SetupTarget> targets`, `List<ItemData> items` |
| ContextMenu | `Copy All To Target` — `ItemCopyUtility` に委譲 |
| 対応Editor | `Item_RandomiserEditor`（2タブ: 位置調整 / アイテム入れ替え） |

#### `ItemSetupScript.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | アイテムをソースからターゲットへコピーするシンプルなスクリプト。 |
| 継承 | `MonoBehaviour`（MA非依存） |
| 持っているデータ | `List<ItemData> items` |
| ContextMenu | `Copy All To Target` — `ItemCopyUtility` に委譲 |
| 対応Editor | `ItemSetupScriptEditor` |

---

### 🎨 Shaders

#### `CommonParticle.shader`
| 項目 | 詳細 |
|---|---|
| 役割 | パーティクル用汎用シェーダー。3テクスチャ対応、スクロールUV、Emission切替、Visible/Hide Mask機能。 |
| Shader名 | `moruton/Package/Particle/ComonParticleShader` (※ファイル名は `CommonParticle.shader` にリネームしましたが、既存マテリアルとの互換性維持のため定義名は旧名を維持しています) |
| 主な機能 | 3レイヤーテクスチャ、UVスクロール、Emission切替、Visible Mask (Tiling/Scale)、Hide Mask、BlendMode/Cull/ZWrite切替 |

---

## Editor Layer

### 🔄 Editor/Core/UpdateChecker（自動アップデート）

> 独立asmdef `MorutonGimmicks.UpdateChecker.Editor`。MA/VRC SDKへの参照なしで動作。

#### `SemVer.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | SemVer 2.0 準拠のバージョンパーサー。prerelease (`0.3.0-beta.1`) や build metadata (`+build.123`) を正しく比較。 |
| 型 | `readonly struct SemVer` |
| 主要メソッド | `TryParse()`, `CompareTo()`, `IsPreRelease` |
| 比較ルール | stable > prerelease。prerelease同士は識別子ごとに数値 < 文字列の順で比較 |

#### `GimmicksUpdateChecker.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | VPMリポジトリ (index.json) から最新バージョン取得し、アップデート判定・自動DL＆インストールを行う。 |
| 主要メソッド | `GetLatestVersionAsync()`, `GetLatestVersionCached()`, `PrefetchLatestVersion()`, `GetCurrentVersion()`, `IsUpdateAvailable()`, `DownloadAndInstallUpdateAsync()` |
| キャッシュ | 30分TTL。初期表示時にバックグラウンドPrefetch、3秒後に反映 |
| 安定版のみ通知 | `IsUpdateAvailable()` はprereleaseを無視 |
| 更新フロー | GitHub ReleaseからZIP DL → 展開 → Packages内差分更新 → vpm-manifest.json更新 → AssetDatabase.Refresh |
| 使用箇所 | `MorutonAvatarPackageEditorHelper` |

---

### 🔧 Editor/Common（Editor共通ユーティリティ）

#### `EditorStyleFactory.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | Editor共通のGUIStyleを遅延初期化で生成。 |
| 提供スタイル | `StepButtonStyle`（折りたたみボタン）, `StepLabelStyle`（セクションラベル） |
| 使用箇所 | `MetamorphoseEditor` |

#### `GimmickPrefabUtility.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | Prefab操作の汎用ユーティリティ。 |
| 主要メソッド | `UnpackPrefab()` — Prefab展開 / `InstantiateUnder()` — 指定親にインスタンス化 / `ReplaceChild()` — 子オブジェクト置き換え / `CopyItems()` — 複数アイテムコピー |
| 使用箇所 | `MetamorphoseEditor`, 将来のギミックEditor |

#### `AnimationBuilder.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | AnimationClip生成の汎用ユーティリティ。Enable/Disableアニメーションを一括生成。 |
| 主要メソッド | `CreateToggleAnimations()` — Enable/Disableクリップを生成 / `GetRelativePath()` — root→childの相対パス取得 / `ApplyClipToState()` — AnimatorControllerのステートにクリップ適用 |
| 使用箇所 | `MetamorphoseEditor`, 将来の変身ギミックEditor |

#### `TargetListDrawer.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | SetupTargetリストの汎用Inspector描画。説明文＋選択ボタンを表示。 |
| 主要メソッド | `DrawTargetsList()` — リスト描画 / `DrawTargetsListFromSerialized()` — SerializedProperty版 / `DrawDeveloperMode()` — ターゲットの追加・削除・編集UI |
| 使用箇所 | `GimmickSetupHelperEditor`, `Item_RandomiserEditor` |

#### `ItemListDrawer.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | ItemDataリストの汎用Inspector描画。プレビュー画像付きで表示。 |
| 主要メソッド | `DrawItemsList()` — サイズ調整＋リスト描画 / `DrawItem()` — 個別アイテム描画 |
| 使用箇所 | `ItemSetupScriptEditor`, `Item_RandomiserEditor` |

---

### 🎭 Editor/Avatars（アバター用Editor）

#### `MorutonAvatarPackageEditorHelper.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | 全ギミックEditorの共通ヘッダーUI。ロゴ表示・リンクボタン・アップデート通知。 |
| 提供機能 | `DrawHeader()` — ロゴ+Booth+Discord+バージョンチェック+アップデートバナー |
| アップデートバナー | 自動更新 / VCCで更新 / 手動DL の3択 |
| 使用箇所 | 全CustomEditor (`GimmickSetupHelperEditor`, `MetamorphoseEditor`, `Item_RandomiserEditor`, `ItemSetupScriptEditor`) |

#### `MetamorphoseEditor.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | 変身ギミック（Metamorphose）の専用Inspector。4ステップ構成。 |
| 継承 | `UnityEditor.Editor` → `[CustomEditor(typeof(Metamorphose))]` |
| UI構成 | Step1: 基本設定 / Step2: 変身後衣装（Prefab展開＋部位別装着） / Step3: ギミック色 / Step4: コラボ情報＋ワンピース＋フェード演出 |
| セットアップ処理 | `SetupTransformation()` — アイテム装着 → ワンピース差し替え → コラボアイテム → フェード装着 → アニメーション生成 → MA Merge Animator生成 |
| DeveloperMode | Generate Animations / Full Re-process |
| 多言語対応 | 5言語（ja/en/ko/it/es）、Toolbar切替 |
| 使用する共通クラス | `AnimationBuilder`, `GimmickPrefabUtility`, `EditorStyleFactory` |

#### `GimmickSetupHelperEditor.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | GimmickSetupHelper用Inspector。ターゲット一覧を表示し、選択・Ping・SceneViewフォーカスが可能。 |
| 使用する共通クラス | `TargetListDrawer` |

#### `Item_RandomiserEditor.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | Item_Randomiser用Inspector。2タブ構成（位置調整 / アイテム入れ替え）。 |
| 使用する共通クラス | `TargetListDrawer`, `ItemListDrawer` |

#### `ItemSetupScriptEditor.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | ItemSetupScript用Inspector。アイテム一覧のプレビュー表示＋Copyボタン。 |
| 使用する共通クラス | `ItemListDrawer` |

#### `LocalizationManager.cs`
| 項目 | 詳細 |
|---|---|
| 役割 | JSON多言語対応管理。キー→翻訳テキストのキャッシュ。 |
| 対応言語 | 日本語 (ja), English (en), 한국어 (ko), Italiano (it), Español (es) |
| 主要メソッド | `Load(scriptName, langCode)` — JSON読み込み / `Get(scriptName, key)` — スクリプト固有テキスト取得 / `GetCommon(key)` — 共通テキスト取得 |
| フォールバック | キーが見つからない場合はキー文字列をそのまま返す |
| 使用箇所 | `MetamorphoseEditor` |

---

## 🌐 ローカリゼーション

### ファイル構成
```
Localization/
├── Common/
│   ├── ja.json    # 共通テキスト（日本語）
│   ├── en.json    # 共通テキスト（英語）
│   ├── ko.json    # 共通テキスト（韓国語）
│   ├── it.json    # 共通テキスト（イタリア語）
│   └── es.json    # 共通テキスト（スペイン語）
└── Metamorphose/
    ├── ja.json    # 変身ギミック固有テキスト
    ├── en.json
    ├── ko.json
    ├── it.json
    └── es.json
```

### 検索順序
1. スクリプト固有テキスト (`Metamorphose/ja.json`)
2. 共通テキスト (`Common/ja.json`)
3. フォールバック（キー文字列そのまま）

---

## 🔗 依存関係図

```
MorutonGimmickPackage (基底・未使用)
    └── MorutonAvatarPackage (基底・MA切替)
            ├── GimmickSetupHelper  → SetupTarget
            └── Item_Randomiser     → SetupTarget, ItemData, ItemCopyUtility

ItemSetupScript (MonoBehaviour直継承)  → ItemData, ItemCopyUtility

Metamorphose (AvatarTagComponent直継承)
    └── MetamorphoseEditor → AnimationBuilder, GimmickPrefabUtility, EditorStyleFactory

全Editor → MorutonAvatarPackageEditorHelper → GimmicksUpdateChecker → SemVer

GimmickSetupHelperEditor → TargetListDrawer
ItemSetupScriptEditor    → ItemListDrawer
Item_RandomiserEditor     → TargetListDrawer, ItemListDrawer

MetamorphoseEditor       → LocalizationManager → JSON files
```

---

## 📊 asmdef一覧

| asmdef | ルート名前空間 | プラットフォーム | 依存 |
|---|---|---|---|
| `com.moruton.gimmicks` | — | すべて | VRC.SDKBase, nadena.dev.modular-avatar.core |
| `com.moruton.gimmicks.Editor` | `Moruton.Gimmicks.Editor` | Editor | ↑ + MA + UpdateChecker |
| `MorutonGimmicks.UpdateChecker.Editor` | `Moruton.Gimmicks.Core` | Editor | なし（独立） |

---

## 🔮 新しい変身ギミックを作るとき

1. `Runtime/Avatars/` に新しいデータ保持クラスを作成
2. `Editor/Avatars/` に `[CustomEditor]` を作成
3. `Editor/Common/` の以下を活用:
   - `AnimationBuilder` — Enable/Disableアニメーション生成
   - `GimmickPrefabUtility` — Prefab展開・子置き換え
   - `EditorStyleFactory` — Step UIの共通スタイル
4. `MorutonAvatarPackageEditorHelper.DrawHeader()` で共通ヘッダーを表示
