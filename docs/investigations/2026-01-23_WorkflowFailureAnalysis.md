# 調査報告: GitHub Actions 失敗原因の分析 (2026-01-23)

## 概要

VPMパッケージ公開用ワークフロー (`.github/workflows/release.yml`) が失敗する原因の調査。
新しい方式（Direct Push）と古い方式（GitHub Pages Plugin）を比較し、不具合の根本原因を特定した。

## 詳細分析

### 1. ワークフローの比較

| 機能 | 旧方式 (成功) | 新方式 (失敗) |
| :--- | :--- | :--- |
| **チェックアウト** | 標準 (タグのコミット) | `ref: main` を強制指定 |
| **VPM更新ツール** | `vrc-get` | `vrc-get` |
| **index.json 公開** | `gh-pages` ブランチへデプロイ | `main` ブランチへ `git push` |
| **ブランチ保護の影響** | 受けにくい (別ブランチ) | **影響大 (mainは保護が一般的)** |

### 2. 特定された原因

#### 原因①: ブランチ保護ルールによる push 拒否 (最有力)

現在の `release.yml` は、更新された `index.json` を `main` ブランチに直接 `git push` しようとしています。
GitHubでは通常 `main` ブランチは保護されており、CI（GitHub Actions）からの直接プッシュは「Push declined (protected branch)」として拒否されます。これが「Actionsが完走しない（失敗する）」最大の理由です。

#### 原因②: チェックアウト指定の不整合

`ref: main` を指定しているため、タグを打った瞬間のコードではなく、その時点での `main` の最新コードをソースとして Zip が作成されます。タグと内容が乖離するリスクがあり、また Git 管理上の衝突（Conflict）を引き起こす要因になります。

#### 原因③: リファクタリングの影響について

「破壊的リファクタリング（名前空間の変更等）」は、**本 Actions の失敗には直接関係ありません**。このワークフローはコンパイルを行わず、ファイルを集めて Zip にし、JSONを書き換えるだけの操作であるため、C#コードの中身がどうなっているかは関知しません。

## 修正指針・解決策

### 解決策A: 確実に成功していた「旧方式」へ戻す (推奨)

`peaceiris/actions-gh-pages` を使用する方式に戻します。

- `index.json` の管理を `gh-pages` ブランチに任せることで、`main` を汚さず、かつ権限エラーも回避できます。

### 解決策B: どうしても `main` に残したい場合

- GitHubリポジトリの設定で、`GITHUB_TOKEN` にブランチ保護を無視する権限を与えるか、強力の権限を持つシークレット（PAT）を使用する必要があります（非推奨）。

## 実装AIへの指示案 (Prompt)

```text
.github/workflows/release.yml を、ブランチ保護の影響を受けにくい「GitHub Pages デプロイ方式 (peaceiris/actions-gh-pages使用)」に戻してください。
具体的には：
1. checkout 時の ref: main 指定を削除する。
2. git push origin main を削除する。
3. peaceiris/actions-gh-pages@v3 を使用して、index.json を含む全ファイルを gh-pages ブランチにプッシュするステップを追加する。
4. vrc-get の引数を、以前成功していた形式に近づける（必要に応じて調整）。
```
