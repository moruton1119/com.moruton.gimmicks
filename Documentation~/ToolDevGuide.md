# Morulab Tool Development Standard

Morulab Launcher に対応したツールを開発するための標準ガイドラインです。
今後新しいツールを作成・移行する際は、本ドキュメントに従ってください。

## 1. ファイル構成

ツールは基本的に **UI Toolkit** で作成し、ロジックとビューを分離します。

```text
Assets/MorulabTools/[ToolName]/Editor/
  ├── [ToolName]Window.cs       # EditorWindow (ガワ), メニュー項目, Launcher連携
  ├── [ToolName]App.cs          # UI構築ロジック, イベントハンドリング (VisualElementを返す)
  ├── [ToolName]Window.uxml     # 構造定義
  ├── [ToolName]Window.uss      # スタイル定義
  ├── [ToolName].md             # ドキュメント (日本語/Default)
  ├── [ToolName]_en.md          # ドキュメント (英語)
  └── [ToolName]_ko.md          # ドキュメント (韓国語)
```

## 2. 実装要件

### A. Windowクラス ([ToolName]Window.cs)

`EditorWindow` を継承し、以下の責務を持ちます。

1. `[MenuItem]` による起動。
2. `[ToolLocalize]` による多言語タイトル・説明定義。
3. `CreateEmbeddedView` 静的メソッドの実装（Launcher埋め込み用）。

```csharp
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MorulabTools.Launcher; // 必須

public class MyToolWindow : EditorWindow
{
    // 1. メニュー項目 & 多言語定義
    [MenuItem("Morulab/Category/MyTool")]
    [ToolLocalize("ja", "ツール名", "これは日本語の説明です。", "カテゴリ名")]
    [ToolLocalize("ko", "도구 이름", "이것은 한국어 설명입니다.", "카테고리 이름")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<MyToolWindow>();
        wnd.titleContent = new GUIContent("MyTool"); // フォールバック英語
    }

    // 2. 通常起動 (EditorWindow)
    public void CreateGUI()
    {
        // Appロジックに rootVisualElement を渡して構築
        new MyToolApp(rootVisualElement); 
    }

    // 3. ランチャー埋め込み対応 (必須)
    // 戻り値は VisualElement, 引数なし, public static
    public static VisualElement CreateEmbeddedView()
    {
        var root = new VisualElement();
        new MyToolApp(root); // 同じロジックを再利用
        return root;
    }
}
```

### B. Appロジック ([ToolName]App.cs)

`EditorWindow` に依存せず、`VisualElement` に対して UI を構築するクラスです。

```csharp
using UnityEngine.UIElements;
using UnityEditor;

public class MyToolApp
{
    public MyToolApp(VisualElement root)
    {
        // UXML/USS のロード
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Path/To/UXML");
        visualTree.CloneTree(root);
        
        // イベント登録や初期化処理
        root.Q<Button>("MyButton").clicked += OnClicked;
    }
}
```

## 3. ドキュメント規則 (Markdown)

各ツールのフォルダに Markdown ファイルを配置すると、ランチャーのサイドパネルに表示されます。

* `MyTool.md` : **日本語 (デフォルト)**
* `MyTool_en.md` : **英語**
* `MyTool_ko.md` : **韓国語**

## 4. デザイン規則 (USS)

* 背景色: `#202020` (Dark Theme)
* 文字色: `#E0E0E0`
* プライマリボタン: `#3f8aff` (Blue)
* Unity標準のスタイルは極力使わず、`MorulabLauncher` と統一感のあるデザインを心がけること。
