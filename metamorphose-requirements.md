# Metamorphose（変身ギミック） 要件定義

## 概要

`com.moruton.gimmicks` パッケージに含まれる変身ギミック（旧称: PrettyCureMirror → Metamorphose）の要件定義書。

---

## 🎯 大目標

| # | 目標 | 説明 |
|---|---|---|
| **1** | 自動アップデート機能の追加 | パッケージのバージョンチェック＋更新通知（VPM対応） |
| **2** | 変身ギミックの軽量化＋リファクタリング | 下記詳細参照 |
| **3** | ワールドプロジェクトでも使えるようにする | Avatar専用 → Avatar/World両対応へ拡張 |

---

## 📋 要件2の詳細（軽量化＋リファクタリング）

### 2-1: スクリプト名変更

- `PrettyCureMirror` → `Metamorphose` にリネーム
- 対象: クラス名・ファイル名・フォルダ名・AddComponentMenu・UI表示名
- **状態: 見送り（キャンセル）**（既存プロジェクトとの後方互換性を維持するため、クラス名 `PrettyCureMirror` のまま維持することが決定しました）

### 2-2: avatar / animator 自動アサイン

- `avatar` が未設定 → 親階層を辿って `VRC_AvatarDescriptor` を検索し、自動でアサイン
- `animator` が未設定 → 見つけたアバターから `Animator` を取得して自動アサイン
- タイミング: コンポーネント追加時（Reset）+ Inspector表示時（OnEnable）
- **状態: 実装済み**（`PrettyCureMirror.cs` 内の `AutoAssignAvatarAndAnimatorIfEmpty()` にて実装済み）

### 2-3: NDMFビルド時配置

- Step 2（衣装アイテム）とStep 4（フェード演出アイテム）をビルド時に自動配置
- 配置のみ（削除なし・Prefab解除なし）
- NDMFがアバターのクローンに対して動くため、元のシーンは汚さない
- **状態: 実装済み**（`MetamorphoseApplyPass.cs` + `ItemPlacer.cs`）

### 2-4: 削除機能の排除

- 以下のメソッドを全て削除:
  - `ReplaceOnePieceChild`（既存の子を削除してから配置）
  - `ProcessItemAttachment`（既存アイテム削除＋新規配置）
  - `ProcessFadeAttachment`（既存フェードアイテム削除＋新規配置）
  - `UnpackPrefab`（Prefab解除）
- **状態: 実装済み**

### 2-5: Setupボタン削除

- NDMFがビルド時に自動処理するため、エディターの「Setup Transformation」ボタンは不要
- UXML・C#両方から削除
- **状態: 実装済み**

### 2-6: ItemPlacer汎用化

- 配置ロジックを共通ユーティリティとして切り出し
- 場所: `Editor/Common/ItemPlacer.cs`
- 提供メソッド:
  - `PlaceItems(target, items)` — 配列をターゲットの子として配置
  - `PlaceItems(target, items, material)` — 配置＋マテリアル差し替え
  - `Place(target, item)` — 単一アイテム配置
  - `ApplyMaterial(item, material)` — マテリアル差し替えのみ
- 今後の別Scriptでも使い回し可能
- **状態: 実装済み**

### 2-7: UI改善

- 選択したアイテムの横並びプレビュー表示
- 各部位（Head/Body/Hand/Leg）のPropertyField下部にサムネイル表示
- 最大4件まで表示、超過分は「+N more...」と表示
- **状態: 実装済み**（`MetamorphoseWindow.cs` 等でプレビュー機能が実装されています）

### 2-8: NDMFビルド時アニメーション自動生成とマージ

- `offTargets`（変身前の衣装）および `model` のON/OFFを切り替えるアニメーションクリップをメモリ上に自動生成
- 生成されたクリップをマージ用のAnimatorControllerへ適用し、Modular Avatarの `ModularAvatarMergeAnimator` に自動割り当てしてマージを完了する
- **状態: 実装済み**（`MetamorphoseApplyPass.cs` の `GenerateAnimations()` で実装）

### 2-9: ギミック色変更のUndoと即時プレビュー

- カラーピッカーによる `gimmickColor` 変更時、エディター上でUndoに対応し、リアルタイムで全パーティクルの `startColor` がプレビュー変更されるようにコールバックを紐付け
- **状態: 実装済み**（`MetamorphoseWindow.cs` のカラー変更コールバックおよび `MetamorphoseSetupService.ApplyGimmickColor()` のUndo対応）

### 2-10: 部位非表示トグルのプレハブ保存化とウィンドウ自動追従

- Devモードの「部位表示切替」ボタンの状態（フラグ値）が、グローバルな `EditorPrefs` ではなくプレハブ（またはシーン上の個別インスタンス）ごとに独立してシリアライズ保存されるよう、`PrettyCureMirror` クラス内に `showHead` などの変数を定義して移行
- エディタウィンドウが開いている際、Unityヒエラルキーの選択変更に自動追従して編集ターゲットを切り替える `OnSelectionChange()` を実装
- **状態: 実装済み**（`PrettyCureMirror.cs`、`MetamorphoseWindow.cs` でのプロパティ保存化および `OnSelectionChange` 実装）

---

## 🏗️ 現在のファイル構成

```
Runtime/
  Avatars/
    PrettyCureMirror.cs          — データモデル + 自動アサイン（PrettyCureMirror のまま名前維持）

Editor/
  Common/
    ItemPlacer.cs                — 共通配置ユーティリティ
  Avatars/
    Metamorphose/
      MetamorphoseEditor.cs      — Inspector UI（UI Toolkit）
      MetamorphoseEditor.uxml    — UXMLレイアウト
      MetamorphoseEditor.uss     — USSスタイル
      MetamorphoseSetupService.cs — ApplyGimmickColorのみ
      MetamorphoseWindow.cs      — 変身ギミック用のメインセットアップウィンドウ
      MetamorphoseWindow.uxml    — ウィンドウレイアウト
      MetamorphoseWindow.uss     — ウィンドウスタイル
  NDMF/
    MetamorphosePlugin.cs        — NDMFプラグイン登録
    MetamorphoseApplyPass.cs     — ビルド時配置処理
    com.moruton.gimmicks.NDMF.asmdef
```

---

## 📐 アーキテクチャ方針

- **エディター**: 設定画面＋プレビュー表示のみ（副作用なし）
- **NDMFビルド時**: アイテム配置のみ（削除なし・Prefab解除なし）
- **共通ユーティリティ**: ItemPlacerを他のScriptでも再利用可能にする
- **アニメーション生成**: 直接AnimatorControllerを書き換えず、MA MergeAnimator等を検討
- **ローカライゼーション**: ja/en/ko/it/es の5言語対応

---

## 📝 メモ

- アニメーション生成ロジックはビルド時にNDMFパスを通じてメモリ上で非破壊的にクローン・マージされるため、元のAnimatorControllerアセット自体は汚れません。
- MetamorphoseSetupService.cs は ApplyGimmickColor のみ残存。
- NDMF asmdef は `com.moruton.gimmicks.Editor` を参照している（ItemPlacerを使用するため）。

---

*最終更新: 2026-06-24 package.json 0.3.0-beta.67時点*
