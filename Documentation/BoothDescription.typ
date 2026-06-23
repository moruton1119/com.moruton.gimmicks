#set page(width: 800pt, height: auto, margin: 40pt)
#set text(font: ("Yu Gothic", "Meiryo"), size: 14pt)

#show heading: it => [
  #set text(fill: rgb("#3a86ff"))
  #block(inset: (bottom: 5pt), stroke: (bottom: 2pt + rgb("#3a86ff")))[
    #it
  ]
]

= Moruton Gimmicks Package

Moruton Laboratory のアバター向けギミック・ツール共通基底パッケージです。
主に VRChat アバター向けの便利なツールや、自作ギミックの基盤となるスクリプトが含まれています。

== 🌟 主な特徴

- *Modular Avatar 自動対応*:
  - MA導入環境では自動的に機能を統合し、非導入環境でも通常のMonoBehaviourとして動作。
- *もルラボのギミックの導入補助ツール*:
  - 煩雑なアバターへのオブジェクト配置や設定を直感的に行える機能群。
- *エディタ内アップデート通知*:
  - 最新バージョンへの更新をUnity Inspector上からワンクリックで実行可能。

== 📦 収録内容

- *ギミック導入時におけるアイテムの位置変更を簡単にするツール*:
  - アイテムの微調整や配置を効率化。
- *ギミック導入時におけるアイテムの入れ替えを簡単にするツール*:
  - 複数のアイテムをワンクリックで入れ替え。
- *ギミック導入時における複雑なセットアップを簡単にするツール*:
  - 煩雑なコンポーネント設定や関連付けを簡略化。

== 🚀 インストール方法

VCC (VRChat Package Manager) 経由での導入を推奨しています。

1. [公式配布ページ](https://moruton1119.github.io/com.moruton.gimmicks/) からリポジトリを追加。
2. VCCでプロジェクトにインストール。

== 🛠 使い方

本パッケージは、対応するもルラボ製ギミックを導入・使用する際に必要となる共通基底パッケージです。
対応ギミックをご購入・導入される際に、併せて本パッケージをプロジェクトにインストールしてご使用ください。

#v(20pt)
#align(center)[
  *制作者*: Moruton ([moruton1119](https://x.com/moruton1119)) \
  *Booth*: #link("https://moruton.booth.pm/")[moruton.booth.pm] \
  *GitHub*: #link("https://github.com/moruton1119/com.moruton.gimmicks")[github.com/moruton1119/com.moruton.gimmicks]
]


