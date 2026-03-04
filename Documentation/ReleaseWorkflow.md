# リリースワークフロー手順

## 概要

このプロジェクトはGitHub Actionsを使用してVPMパッケージを自動リリースしています。

## ブランチ戦略

| ブランチ | 用途 | バージョン例 |
|----------|------|--------------|
| `main` | 正式版リリース | `0.3.0` |
| `Dev` | ベータ版リリース | `0.3.0-beta.1` |

### コミット先の自動判定

- **正式版**（`-` を含まない）→ `main` ブランチにコミット
- **ベータ版**（`-` を含む）→ `Dev` ブランチにコミット

## 正式版リリース

### 1. mainブランチで作業

```bash
git checkout main
git pull origin main
```

### 2. package.jsonのバージョンを更新

```json
{
    "version": "0.3.0"
}
```

### 3. タグをプッシュ

```bash
git add package.json
git commit -m "chore: bump to 0.3.0"
git tag v0.3.0
git push origin main --tags
```

## ベータ版リリース

### 1. Devブランチで作業

```bash
git checkout Dev
git pull origin Dev
```

### 2. package.jsonのバージョンを更新

```json
{
    "version": "0.3.0-beta.1"
}
```

### 3. タグをプッシュ

```bash
git add package.json
git commit -m "chore: bump to 0.3.0-beta.1"
git tag v0.3.0-beta.1
git push origin Dev --tags
```

## 自動実行される処理

GitHub Actionsが以下を自動的に行います：

1. **パッケージの作成**
   - `com.moruton.gimmicks-{version}.zip` を作成
   - 除外対象: `.git/`, `.github/`, `.vscode/`, `Tests/`, `obj/`

2. **GitHub Releaseの作成**
   - タグ名に `-` が含まれる場合（例: `v0.3.0-beta.1`）はプレリリースとして作成
   - それ以外は正式リリースとして作成

3. **VPMリポジトリの更新**
   - `package.json` のバージョンを更新
   - `docs/index.json` を更新
   - 新しいバージョン情報を追加
   - **正式版**: 変更を `main` ブランチにコミット・プッシュ
   - **ベータ版**: 変更を `Dev` ブランチにコミット・プッシュ

## VPMリポジトリURL

```
https://moruton1119.github.io/com.moruton.gimmicks/index.json
```

このURLは `docs/index.json` を指しています。

## VCCでの扱い

| ユーザー | 正式版 | ベータ版 |
|----------|--------|----------|
| **VCCデフォルト** | 自動表示・更新 | 非表示 |
| **「Include Prerelease」有効** | 表示 | 手動選択でインストール可能 |
| **Editorスクリプト** | 更新通知あり | チェックしない |

### Editorスクリプトの動作

`MorutonAvatarPackageEditorHelper.cs` は `main` ブランチの `package.json` を参照しているため、正式版のみ更新通知を行います。

```csharp
private const string RemotePackageJsonUrl = 
    "https://raw.githubusercontent.com/moruton1119/com.moruton.gimmicks/main/package.json";
```

## 注意事項

- リリース前に `package.json` のバージョンを更新する必要があります
- `docs/index.json` は自動的に更新されるため、手動で編集しないでください
- 古いバージョン情報は保持されます
- **ベータ版リリースは必ず `Dev` ブランチで行ってください**
- **正式版リリースは必ず `main` ブランチで行ってください**

## ファイル構成

```
com.moruton.gimmicks/
├── .github/
│   └── workflows/
│       └── release.yml    # リリースワークフロー定義
├── docs/
│   └── index.json         # VPMリポジトリインデックス（GitHub Pages）
├── package.json           # パッケージ情報
└── ...
```

## トラブルシューティング

### リリースが失敗する場合

1. タグが正しい形式か確認（`v*` の形式が必要）
2. GitHub Actionsのログを確認
3. `docs/index.json` が正しく更新されているか確認

### index.jsonが更新されない場合

- main/Devブランチへのプッシュ権限を確認
- コンフリクトが発生していないか確認

### ベータ版がVCCに表示されない場合

- VCCで「Include Prerelease」オプションが有効か確認
- `docs/index.json` にベータ版が登録されているか確認

### 間違ったブランチでリリースした場合

1. GitHub Releaseを削除
2. タグを削除
3. 正しいブランチで再度リリース
