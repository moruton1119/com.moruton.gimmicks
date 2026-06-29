# 絶対ルール：色のハードコード禁止

## 背景
ネオン（AIアシスタント）が過去に何度も色のハードコードを繰り返し、もるとんの時間を大幅に無駄にした。
具体的な被害：
- C#インラインスタイルで28箇所の色を直接指定 → USS変更が反映されない
- 「ハードコードしてない」と言いながらインラインスタイルで色を上書き
- 原因調査前に手動でzipを作成したりindex.jsonを書き換えて状況を悪化

## 絶対にやってはいけないこと

### ❌ 1. C#でのインラインスタイル色指定
```csharp
// 絶対ダメ
element.style.backgroundColor = new Color(1f, 0f, 0f);
element.style.color = theme.accent;
hb.style.borderLeftColor = theme.accent;
```

### ❌ 2. テーマIDによる分岐
```csharp
// 絶対ダメ
if (theme.id == "Daylight") { ... }
```

### ❌ 3. 「USSが効かないからC#で上書きする」という発想
USSが効かないなら、USSのセレクタ詳細度を上げるか、
USSクラスを親要素に付与してcascadeを届かせる。

## 守るべきルール

### ✅ 色の制御は100% USS（CSS変数）で行う

1. **EditorThemeDefinition.cs** — 色の「定義」のみ（C#のデータ構造）
2. **Theme_Moonlight.uss / Theme_Daylight.uss** — CSS変数の値と、各要素への`var()`適用
3. **MetamorphoseWindow.Theme.cs** — C#側は**テーマクラスの付与のみ**

```csharp
// ✅ 正しい：テーマクラスを付与するだけ
_root.AddToClassList(theme.ussClassName);
```

### 色を変えたい時の手順

1. **Theme_Daylight.uss** または **Theme_Moonlight.uss** のCSS変数を変更
2. または **EditorThemeDefinition.cs** の該当テーマの色フィールドを変更
3. C#ファイルは原則として触らない

### チェックリスト（コミット前に必ず確認）

- [ ] C#ファイルに `style.color` / `style.backgroundColor` / `style.border*Color` の**色値直接指定**がないか
- [ ] `theme.id ==` による色分岐がないか
- [ ] `new Color(` がインラインスタイル内にないか（オープニング演出のEditorThemeDefinition経由はOK）
- [ ] 変更した色がUSS（CSS変数）経由で適用されるか

## 例外：インラインスタイルで色以外を設定するのはOK

```csharp
// ✅ OK：レイアウト・サイズ・パディング等
hb.style.borderLeftWidth = 3f;
hb.style.paddingLeft = 8f;
hb.style.marginTop = 4f;
```

色以外（サイズ、パディング、マージン、角丸など）のインラインスタイルは許可する。
ただし、それらもUSSに移行できるならUSSでする方が良い。

## ネオンへ

このドキュメントを読んだら、今後**一切**色のハードコードをしないこと。
もるとんの信頼を取り戻すまで、色に関わる変更は必ずUSSファイルのみで行うこと。
