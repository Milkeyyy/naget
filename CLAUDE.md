# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.


## Context7 の使用
コード生成、セットアップや設定手順、ライブラリ/APIドキュメントが必要な場合は、常にContext7を使用してください。つまり、私が明示的に指示しなくても、Context7 MCPツールを使ってライブラリIDを解決し、ライブラリドキュメントを取得できるようにしてください。


## プロジェクト概要

**naget** は Avalonia 11 ベースのクロスプラットフォーム検索ランチャーアプリケーション。
対応プラットフォーム: Windows (x64, arm64)、macOS (arm64)
言語: C# / .NET 8.0

## ビルドコマンド

NUKEビルドシステムを使用。

```bash
# Windows ビルド (x64)
./build.cmd --runtime win-x64 --releasechannel nightly --releasenumber <番号>

# Windows ビルド (arm64)
./build.cmd --runtime win-arm64 --releasechannel nightly --releasenumber <番号>

# macOS ビルド (arm64)
./build.sh --runtime osx-arm64 --releasechannel nightly --releasenumber <番号>

# 通常の dotnet ビルド (開発時)
dotnet build naget/naget.csproj

# 通常の dotnet パブリッシュ
dotnet publish naget/naget.csproj -c Release -r win-x64 --no-self-contained
```

### リリースチャネル
`--releasechannel` には `nightly` / `alpha` / `release` を指定。

### ビルド出力
`_Pack/{runtime}/Build/` に出力される。

## アーキテクチャ

### MVVMパターン
- **Epoxy.Avalonia11** による MVVM 実装
- Compiled Bindings デフォルト有効 (`AvaloniaUseCompiledBindingsByDefault=true`)
- View → ViewModel → Model の依存方向

### 主要レイヤー

| レイヤー | パス | 役割 |
|--------|------|------|
| Views | `naget/Views/` | Avalonia AXAML UI定義 |
| ViewModels | `naget/ViewModels/` | UI ロジック・状態管理 |
| Models | `naget/Models/` | データモデル（設定、検索エンジン） |
| Helpers | `naget/Helpers/` | グローバルホットキー、ロギング、Velopack更新 |
| Services | `naget/Services/` | ウィンドウ管理 |

### 設定管理
- `naget/Models/Config/ConfigManager.cs` が JSON 設定ファイルの読み書きを一元管理
- `naget/Models/Config/HotKey/` にホットキー設定のモデルとマネージャー

### 更新機能
- Velopack (`naget/Helpers/Updater.cs`) による自動更新
- `App.axaml.cs` の起動時に Velopack を初期化
- リリースは GitHub Releases へ Velopack チャネル別にアップロード

### グローバルホットキー
- SharpHook ライブラリを使用
- `naget/Helpers/HotKeyHelper.cs` でシステムワイドのキー検出を実装

### インアプリブラウザ
- WebViewControl-Avalonia (Chromium Embedded Framework) によるブラウザーコントロール
- `naget/CEF_Resources/` に win-x64 / win-arm64 用のバイナリを同梱

## バージョン管理

`naget/build.json` にバージョン情報を保持。ビルド時に `Build.cs` が更新する。

```json
{
  "version": "1.0.0",
  "release_channel": "nightly",
  "release_number": "<コミットハッシュ or 番号>",
  "full_version": "1.0.0-nightly.<release_number>"
}
```

## コーディング規約

`.editorconfig` に従う:
- インデント: **タブ**
- 改行コード: CRLF
- 文字コード: UTF-8-BOM

## CI/CD

- `dev` ブランチへのプッシュで Nightly ビルドが自動実行 (`build-nightly.yml`)
- ビルド後 `vpk` コマンドで Velopack パッケージを作成し GitHub Releases へアップロード
- チャネル名は `{os}-{arch}-{channel}` 形式 (例: `win-x64-nightly`)
