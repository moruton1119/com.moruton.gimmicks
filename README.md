# Moruton Gimmicks Package

**[➡️ VCCに追加](https://moruton1119.github.io/com.moruton.gimmicks/)**

Moruton Laboratory のアバター用ギミックパッケージです。変身ギミック（Metamorphose）を中心に、Modular Avatarと連携した各種ギミックを提供します。

## インストール

VCCに以下のURLを追加してください：

```
https://moruton1119.github.io/com.moruton.gimmicks/index.json
```

または [VCCリンク](vcc://vpm/addRepo?url=https://moruton1119.github.io/com.moruton.gimmicks/index.json) をクリック。

## 必須パッケージ

- VRChat Avatars SDK
- Modular Avatar
- NDMF (Modular Avatarが自動で入れます)

## 主な機能

### 変身ギミック（Metamorphose）

- 衣装の切り替えアニメーションを自動生成
- パーツごとのドラッグ&ドロップ設定
- ProtectedAnimationSystem：暗号化DLLからアニメーションを復元・注入
- 5つのUIテーマ（Moonlight / Daylight / Cyber / Wizard / Diamond）
- 5言語対応（日本語 / English / 韓国語 / イタリア語 / スペイン語）
- 魔法少女風オープニング演出

### Dev Pose Placement（Editor専用・beta）

> beta機能です。分離コンパイルと静的検査は実施済みですが、実Unity Editorでの動作は未検証です。

- Scene上のアバターの子へ `Morulab/Avatars/Dev Pose Placement (Editor Only)` を追加し、Humanoid muscle-onlyのAnimationClipを指定します。
- Inspectorの「Dev開始」で親アバターへポーズを固定し、Unity標準の移動・回転ツールで小物を配置します。時間指定もInspectorで行います。「Dev停止」で人体骨だけを元へ戻し、小物のローカル配置は保持します。
- Editモード専用です。Play移行、domain reload、Editor終了、Scene保存前には自動停止します。他のAnimationModeを開始・停止しません。
- 旧Assets版から移行する場合、このパッケージ導入後に同じGUID/型が重複しないよう、Scene参照を確認してから旧 `DevPosePlacement` フォルダをプロジェクト外へ退避または削除してください。パッケージ内の3スクリプトは旧版のmeta GUIDを維持しています。

### その他のギミック

- **Item Randomiser** — アイテムのランダム切り替え
- **Item Setup Script** — アイテムの一括セットアップ
- **Gimmick Setup Helper** — セットアップ対象の管理

## ドキュメント

- [アーキテクチャ](Documentation/Architecture.md)
- [リリースワークフロー](Documentation/ReleaseWorkflow.md)
- [ProtectedAnimationSystem設計書](Documentation/ProtectedAnimationSystem.md)
- [色のハードコード禁止ルール](Documentation/AntiHardcodeRules.md)

## リンク

- [BOOTH](https://moruton.booth.pm/)
- [X (Twitter)](https://x.com/MoruLabo)
- [Note](https://note.com/mortonlaboratory)
- [Discord](https://discord.gg/GHJwmyTcfX)
