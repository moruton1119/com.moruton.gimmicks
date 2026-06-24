# バナー広告の変更方法

Metamorphose Setup Window の下部に表示されるバナー広告（他ギミックの紹介）の変更方法。

---

## 方法1: デフォルトURLを編集（全ユーザーに反映）

パッケージ内のデフォルトURL配列を編集してリリースする。

### ファイル

```
Editor/Avatars/Metamorphose/MetamorphoseWindow.cs
```

### 手順

1. `DefaultBannerUrls` 配列を探す（ファイル先頭付近、コメントブロックで囲まれている）

```csharp
// ═══════════════════════════════════════════════════════════
//  バナー広告URL — 変更する場合はここを編集
//  詳細: Documentation/BannerAds.md
// ═══════════════════════════════════════════════════════════
private static readonly string[] DefaultBannerUrls =
{
    "https://moruton.booth.pm/items/6837270",
    "https://moruton.booth.pm/items/7575133",
    // 追加: URLをここに足す
    // 削除: 行を消すだけ
};
```

2. URLを追加・削除・変更する
3. `package.json` のバージョンを上げる
4. コミット → タグ → push

### 表示される内容

各URLから自動的に以下を取得して表示:

- **タイトル**: ページの `og:title` または `<title>` タグ
- **サムネイル画像**: ページの `og:image` メタタグ

取得は非同期で行われ、完了次第カードに反映される。

---

## 方法2: DevModeで個別に上書き（ユーザー個別）

各アバターの Metamorphose コンポーネントで個別にURLを設定可能。

### 手順

1. Metamorphose Setup Window を開く
2. **Dev** ページに移動
3. **Banner Ad URLs** フィールドを編集
4. URLを追加・削除

> DevModeでURLが設定されている場合、デフォルトURLより優先される。
> 空の場合はデフォルトURLが使用される。

---

## 対応しているURL

以下のメタタグに対応しているページなら自動取得可能:

- `og:title` / `twitter:title` / `<title>`
- `og:image` / `twitter:image`

Booth、Twitter、GitHubなど、多くのサイトで動作する。

取得に失敗した場合はURLがそのままタイトルとして表示される。

---

## 卡片（カード）のデザイン調整

バナーの見た目を変更する場合:

```
Editor/Avatars/Metamorphose/MetamorphoseWindow.uss
```

以下のクラスを編集:

| クラス | 説明 |
|--------|------|
| `.banner` | バナー全体の背景・余白 |
| `.banner-card` | カード1枚のサイズ・色 |
| `.banner-card-image` | サムネイル画像のサイズ |
| `.banner-card-label` | タイトルテキストの色・サイズ |
