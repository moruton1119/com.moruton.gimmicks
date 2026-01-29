# VCCパッケージ リリース手順書 (VPM Release Workflow)

本リポジトリでは、GitHub Actions を使用して VCC (VPM) パッケージのリリースを自動化しています。
この自動化ワークフロー (`.github/workflows/release.yml`) により、タグをプッシュするだけで、Releaseの作成・ZIPのアップロード・`index.json`の更新までが全自動で行われます。

## 🚀 リリース手順

新しいバージョンをリリースする際は、以下の手順に従ってください。

### 1. 最新状態の取得

前回のリリースで GitHub Actions (bot) が `index.json` を更新して `main` ブランチにプッシュしているため、必ず最初にプルしてください。

```bash
git pull origin main
```

### 2. バージョンの更新

`package.json` を開き、`version` フィールドを新しいバージョン番号（例: `0.1.12`）に書き換えてください。

```json
{
  "name": "com.moruton.gimmicks",
  "version": "0.1.12",  <-- ここを更新
  ...
}
```

### 3. 変更のコミットとプッシュ

```bash
git add package.json
git commit -m "chore: bump version to 0.1.12"
git push origin main
```

### 4. タグの作成とプッシュ（リリース実行）

タグ `vX.Y.Z` を作成してプッシュすると、GitHub Actions が起動し、リリース処理が始まります。

```bash
git tag v0.1.12
git push origin v0.1.12
```

---

## 🤖 自動化の仕組み

1. **Tag Push検知**: `v*` (例: `v0.1.12`) で始まるタグがプッシュされると、Workflowが起動します。
2. **ZIP作成**: リポジトリの内容をZIPファイルに圧縮します（不要ファイルは除外）。
3. **GitHub Release作成**: GitHubのリリースページにZIPファイルをアップロードします。
4. **index.json自動更新**:
   - `index.json` (ルート) と `docs/index.json` の両方に、新しいバージョン情報を追記します。
   - 更新された `index.json` を `main` ブランチに直接コミット＆プッシュします。
   - **重要**: これにより、GitHub Pages（Web公開）への反映待ち時間の短縮と、反映ミスの防止を図っています。

## ⚠️ トラブルシューティング

### VCCに更新が表示されない場合

1. **コミット確認**: GitHub Actions のログを確認し、`Commit & Push index.json` ステップが成功しているか確認してください。
2. **キャッシュ**: VCCはリポジトリ情報をキャッシュします。VCCの [Settings] -> [Packages] -> [Repositories] から、該当リポジトリの「Update」ボタンを押すか、一度 Remove して再度 Add してください。

### GitHub Actions でエラーが出る場合

- **コンフリクト**: 手順1（git pull）を忘れると、botが更新した `index.json` と手元の変更が衝突し、後のプッシュでエラーになる可能性があります。

---
**Author**: Moruton Laboratory
**Last Updated**: 2026-01-29
