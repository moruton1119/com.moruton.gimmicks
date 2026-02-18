# com.moruton.gimmicks アーキテクチャ

## 全体構成

```mermaid
graph TB
    subgraph "com.moruton.gimmicks"
        subgraph Runtime["Runtime Layer"]
            subgraph Gimmicks["Gimmicks Core"]
                MorutonGimmickPackage["MorutonGimmickPackage<br/>ギミック基底クラス"]
                MorutonAvatarPackage["MorutonAvatarPackage<br/>アバター専用基底クラス"]
                GimmickSetupHelper["GimmickSetupHelper<br/>セットアップ補助"]
            end
            
            subgraph MorulabToolsData["MorulabTools Data"]
                ToolCommandData["ToolCommandData<br/>ツール情報"]
                LocalizedInfo["LocalizedInfo<br/>多言語情報"]
            end
            
            subgraph Attributes["Attributes"]
                MenuDescriptionAttr["MenuDescriptionAttribute<br/>メニュー説明属性"]
                ToolLocalizeAttr["ToolLocalizeAttribute<br/>多言語対応属性"]
            end
        end
        
        subgraph Editor["Editor Layer"]
            MorulabLauncher["MorulabLauncher<br/>統合ツールランチャー"]
            ReflectionUtils["ReflectionUtils<br/>MenuItem自動探索"]
        end
    end
    
    MorutonAvatarPackage -->|継承| MorutonGimmickPackage
    GimmickSetupHelper -->|継承| MorutonAvatarPackage
    
    MorulabLauncher -->|使用| ReflectionUtils
    MorulabLauncher -->|表示| ToolCommandData
    ReflectionUtils -->|生成| ToolCommandData
    ToolCommandData -->|包含| LocalizedInfo
    ReflectionUtils -->|読取| MenuDescriptionAttr
    ReflectionUtils -->|読取| ToolLocalizeAttr
```

## Runtime Layer

### Gimmicks Core

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
        +ギミック共通処理
    }
    
    class MorutonAvatarPackage {
        <<abstract>>
        アバター専用基底
    }
    
    class GimmickSetupHelper {
        +Sprite dummyImage
        +List~SetupTarget~ targets
        +説明文と対象オブジェクト管理
    }
    
    MonoBehaviour <|-- MorutonGimmickPackage : 非MA環境
    AvatarTagComponent <|-- MorutonGimmickPackage : MA環境
    MorutonGimmickPackage <|-- MorutonAvatarPackage
    MorutonAvatarPackage <|-- GimmickSetupHelper
```

### MorulabTools Data & Attributes

```mermaid
classDiagram
    class ToolCommandData {
        +string Path
        +string OriginalTitle
        +Dictionary~string,LocalizedInfo~ LocalizedInfos
        +MethodInfo TargetMethod
        +string IconName
        +GetInfo(lang) LocalizedInfo
    }
    
    class LocalizedInfo {
        +string Title
        +string Description
        +string Category
    }
    
    class MenuDescriptionAttribute {
        +string Description
        +string Category
        +string IconName
    }
    
    class ToolLocalizeAttribute {
        +string Lang
        +string Title
        +string Description
        +string Category
    }
    
    ToolCommandData *-- LocalizedInfo : 包含
    MenuDescriptionAttribute ..> ToolCommandData : メタデータ提供
    ToolLocalizeAttribute ..> ToolCommandData : 多言語情報提供
```

## Editor Layer

### MorulabLauncher フロー

```mermaid
flowchart TD
    A[Unity起動] --> B[MorulabLauncher.ShowWindow]
    B --> C[CreateGUI]
    C --> D[UXML/USS読込]
    D --> E[RefreshToolList]
    
    E --> F[ReflectionUtils.FindCommands]
    F --> G[アセンブリ走査]
    G --> H{MenuItem属性あり?}
    H -->|Yes| I{Morulabパス?}
    H -->|No| G
    I -->|Yes| J[ToolCommandData生成]
    I -->|No| G
    J --> K[属性から多言語情報取得]
    K --> L[カテゴリ別グループ化]
    L --> M[UI描画]
    
    N[ツール選択] --> O[SelectTool]
    O --> P{埋め込み可能?}
    P -->|Yes| Q[CreateEmbeddedView]
    P -->|No| R[ExecuteCommand]
    Q --> S[ツール表示]
    R --> S
```

### ReflectionUtils 処理フロー

```mermaid
sequenceDiagram
    participant Launcher as MorulabLauncher
    participant Utils as ReflectionUtils
    participant Assembly as AppDomain
    participant Method as MethodInfo
    
    Launcher->>Utils: FindCommands("Morulab")
    Utils->>Assembly: GetAssemblies()
    loop 各アセンブリ
        Assembly->>Utils: GetTypes()
        loop 各Type
            Utils->>Method: GetMethods()
            loop 各Method
                Method->>Utils: GetCustomAttributes()
                Note over Utils: MenuItem属性チェック
                Note over Utils: MenuDescriptionAttribute取得
                Note over Utils: ToolLocalizeAttribute取得
                Utils->>Utils: ToolCommandData生成
            end
        end
    end
    Utils->>Launcher: List<ToolCommandData>
```

## 役割まとめ

| コンポーネント | 役割 |
|--------------|------|
| **MorutonGimmickPackage** | ギミックの基底クラス（Modular Avatar自動切替） |
| **MorutonAvatarPackage** | アバター専用ギミックの基底クラス |
| **GimmickSetupHelper** | アバターセットアップ補助コンポーネント |
| **MorulabLauncher** | 統合ツールランチャーUI（多言語対応） |
| **ReflectionUtils** | MenuItem自動検出・ツール登録 |
| **ToolCommandData** | ツール情報データクラス |
| **LocalizedInfo** | 多言語対応情報 |
| **MenuDescriptionAttribute** | メニュー説明付与属性 |
| **ToolLocalizeAttribute** | 多言語対応属性 |
