# com.moruton.gimmicks アーキテクチャ

## 全体構成

```mermaid
graph TB
    subgraph "com.moruton.gimmicks"
        subgraph Runtime["Runtime Layer"]
            subgraph BaseClasses["Base Classes"]
                MorutonGimmickPackage["MorutonGimmickPackage<br/>ギミック基底クラス"]
                MorutonAvatarPackage["MorutonAvatarPackage<br/>アバター専用基底クラス"]
            end
            
            subgraph AvatarComponents["Avatar Components"]
                GimmickSetupHelper["GimmickSetupHelper<br/>セットアップ補助"]
                PrettyCureMirror["PrettyCureMirror<br/>プリキュア変身ギミック"]
                Item_Randomiser["Item_Randomiser<br/>アイテムランダマイザー"]
                ItemSetupScript["ItemSetupScript<br/>アイテムセットアップ"]
            end
        end
        
        subgraph Editor["Editor Layer"]
            LocalizationManager["LocalizationManager<br/>多言語管理"]
            MorutonAvatarPackageEditorHelper["MorutonAvatarPackageEditorHelper<br/>共通UI・更新管理"]
            GimmickSetupHelperEditor["GimmickSetupHelperEditor"]
            PrettyCureMirrorEditor["PrettyCureMirrorEditor"]
            Item_RandomiserEditor["Item_RandomiserEditor"]
            ItemSetupScriptEditor["ItemSetupScriptEditor"]
        end
    end
    
    MorutonAvatarPackage -->|継承| GimmickSetupHelper
    MorutonAvatarPackage -->|継承| Item_Randomiser
    MorutonGimmickPackage -.->|現状未使用| MorutonAvatarPackage
    
    MorutonAvatarPackageEditorHelper -->|使用| LocalizationManager
    GimmickSetupHelperEditor -->|使用| MorutonAvatarPackageEditorHelper
    PrettyCureMirrorEditor -->|使用| MorutonAvatarPackageEditorHelper
    PrettyCureMirrorEditor -->|使用| LocalizationManager
    Item_RandomiserEditor -->|使用| MorutonAvatarPackageEditorHelper
    ItemSetupScriptEditor -->|使用| MorutonAvatarPackageEditorHelper
```

## Runtime Layer

### Base Classes

```mermaid
classDiagram
    class MonoBehaviour {
        +gameObject
        +transform
    }
    
    class AvatarTagComponent {
        <<Modular Avatar>>
    }
    
    class MorutonGimmickPackage {
        <<abstract>>
        条件付き継承: MA有効時はAvatarTagComponent
    }
    
    class MorutonAvatarPackage {
        <<abstract>>
        条件付き継承: MA有効時はAvatarTagComponent
    }
    
    class GimmickSetupHelper {
        +Sprite dummyImage
        +List~SetupTarget~ targets
    }
    
    class PrettyCureMirror {
        +GameObject avatar
        +GameObject model
        +GameObject[] offTargets
        +Animator animator
        +Transform headTarget
        +GameObject[] headItems
        ...多数の設定項目
    }
    
    class Item_Randomiser {
        +List~SetupTarget~ targets
        +List~ItemData~ items
        +CopyAllToTarget()
    }
    
    class ItemSetupScript {
        +List~ItemData~ items
        +CopyAllToTarget()
    }
    
    MonoBehaviour <.. MorutonGimmickPackage : MA無効時
    AvatarTagComponent <.. MorutonGimmickPackage : MA有効時
    MonoBehaviour <.. MorutonAvatarPackage : MA無効時
    AvatarTagComponent <.. MorutonAvatarPackage : MA有効時
    
    MorutonAvatarPackage <|-- GimmickSetupHelper
    AvatarTagComponent <|-- PrettyCureMirror : 直接継承
    MorutonAvatarPackage <|-- Item_Randomiser
    MonoBehaviour <|-- ItemSetupScript : 直接継承
```

### GimmickSetupHelper データ構造

```mermaid
classDiagram
    class GimmickSetupHelper {
        +Sprite dummyImage
        +List~SetupTarget~ targets
    }
    
    class SetupTarget {
        +string description
        +Transform targetObject
    }
    
    GimmickSetupHelper *-- SetupTarget : contains
```

### Item_Randomiser / ItemSetupScript データ構造

```mermaid
classDiagram
    class Item_Randomiser {
        +List~SetupTarget~ targets
        +List~ItemData~ items
        +CopyAllToTarget()
    }
    
    class ItemSetupScript {
        +List~ItemData~ items
        +CopyAllToTarget()
    }
    
    class SetupTarget {
        +string description
        +Transform targetObject
    }
    
    class ItemData {
        +GameObject sourceObject
        +Transform targetParent
    }
    
    Item_Randomiser *-- SetupTarget
    Item_Randomiser *-- ItemData
    ItemSetupScript *-- ItemData
```

## Editor Layer

### LocalizationManager

```mermaid
classDiagram
    class LocalizationManager {
        <<static>>
        -Dictionary~string,Dictionary~ commonTexts
        -Dictionary~string,Dictionary~ scriptTexts
        -string[] supportedLanguageCodes
        -string[] supportedLanguageNames
        -string currentLanguage
        +SupportedLanguageNames$
        +SupportedLanguageCodes$
        +CurrentLanguage$
        +SetLanguage(languageCode)
        +Load(scriptName, languageCode)
        +Get(scriptName, key)$
        +GetCommon(key)$
        -LoadJson(path)
    }
```

対応言語: 日本語 (ja), English (en), 한국어 (ko), Italiano (it), Español (es)

### MorutonAvatarPackageEditorHelper

```mermaid
classDiagram
    class MorutonAvatarPackageEditorHelper {
        <<static>>
        -string latestVersion
        -bool isChecking
        -bool isUpdating
        -string updateStatus
        +DrawHeader()$
        -CheckVersion()$
        -IsNewerVersion(latest, current)$
        -GetCurrentVersion()$
        -FetchRemoteVersion()$
        -StartAutoUpdate()$
        -PerformUpdate()$
        -UpdateVpmManifest(newVersion)$
    }
```

機能:
- ヘッダー描画 (ロゴ、Booth/Discordリンク)
- バージョンチェック (GitHubリリース確認)
- 自動更新 (ZIP ダウンロード → 展開 → 適用)

### Editor Classes 構成

```mermaid
flowchart TD
    subgraph Editors["CustomEditor Classes"]
        GimmickSetupHelperEditor
        PrettyCureMirrorEditor
        Item_RandomiserEditor
        ItemSetupScriptEditor
    end
    
    subgraph Shared["Shared Components"]
        MorutonAvatarPackageEditorHelper
        LocalizationManager
        GimmickSetupHelperEditor.DrawDeveloperMode
        GimmickSetupHelperEditor.DrawTargetsList
        ItemSetupScriptEditor.DrawItemsList
    end
    
    GimmickSetupHelperEditor --> MorutonAvatarPackageEditorHelper
    PrettyCureMirrorEditor --> MorutonAvatarPackageEditorHelper
    PrettyCureMirrorEditor --> LocalizationManager
    Item_RandomiserEditor --> MorutonAvatarPackageEditorHelper
    Item_RandomiserEditor --> GimmickSetupHelperEditor.DrawDeveloperMode
    Item_RandomiserEditor --> ItemSetupScriptEditor.DrawItemsList
    ItemSetupScriptEditor --> MorutonAvatarPackageEditorHelper
```

## ファイル構成

```
com.moruton.gimmicks/
├── Runtime/
│   ├── MorutonGimmickPackage.cs
│   └── Avatars/
│       ├── MorutonAvatarPackage.cs
│       ├── GimmickSetupHelper.cs
│       ├── PrettyCureMirror.cs
│       ├── Item_Randomiser.cs
│       └── ItemSetupScript.cs
├── Editor/
│   └── Avatars/
│       ├── LocalizationManager.cs
│       ├── MorutonAvatarPackageEditorHelper.cs
│       ├── GimmickSetupHelperEditor.cs
│       ├── PrettyCureMirrorEditor.cs
│       ├── Item_RandomiserEditor.cs
│       ├── ItemSetupScriptEditor.cs
│       └── Localization/
│           ├── Common/
│           │   ├── ja.json
│           │   ├── en.json
│           │   ├── ko.json
│           │   ├── it.json
│           │   └── es.json
│           └── PrettyCureMirror/
│               ├── ja.json
│               ├── en.json
│               ├── ko.json
│               ├── it.json
│               └── es.json
└── Documentation/
    └── Architecture.md
```

## 役割まとめ

| コンポーネント | 役割 |
|--------------|------|
| **MorutonGimmickPackage** | ギミックの基底クラス（Modular Avatar自動切替）※現状未使用 |
| **MorutonAvatarPackage** | アバター専用ギミックの基底クラス（Modular Avatar自動切替） |
| **GimmickSetupHelper** | アバターセットアップ補助コンポーネント（説明文・対象管理） |
| **PrettyCureMirror** | プリキュア変身ギミック（衣装切替・アニメーション生成） |
| **Item_Randomiser** | アイテムランダマイザー（ターゲット調整・アイテム入れ替え） |
| **ItemSetupScript** | アイテムセットアップ（ソース→ターゲットコピー） |
| **LocalizationManager** | JSON多言語対応管理（5言語対応） |
| **MorutonAvatarPackageEditorHelper** | 共通ヘッダーUI・バージョンチェック・自動更新 |
| **GimmickSetupHelperEditor** | GimmickSetupHelper用Inspector UI |
| **PrettyCureMirrorEditor** | PrettyCureMirror用Inspector UI（多言語対応） |
| **Item_RandomiserEditor** | Item_Randomiser用Inspector UI（タブ切替） |
| **ItemSetupScriptEditor** | ItemSetupScript用Inspector UI |
