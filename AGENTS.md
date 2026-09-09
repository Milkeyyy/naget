# AGENTS.md

このファイルは OpenCode がリポジトリで作業する際の案内です。

## ビルド・実行

- .NET **10.0** ターゲット (`global.json`: 10.0.104, csproj: `net10.0`)
- 開発ビルド: `dotnet build naget/naget.csproj`
- 製品ビルドは NUKE 経由: `./build.cmd --runtime win-x64 --releasechannel nightly --releasenumber <番号>`
- NUKE のエントリポイントは `build/Build.cs`、既定ターゲットは `Compile` (Restore → `dotnet publish`、framework-dependent + PublishTrimmed)
- ビルド出力は `_Pack/{runtime}/Build/`
- リリース作業 (vpk パッキングと GitHub Releases アップロード) は `Build_win-x64_nightly.bat` / `Build_win-arm64_nightly.bat` / `Build_osx-arm64_nightly.sh` を使用。CI は `.github/workflows/build-nightly.yml`
- **テストプロジェクトは存在しない**。テストコマンドを実行しないこと

## アーキテクチャ

### 単一プロジェクト
`sln` に `naget` と `build/_build.csproj` の2プロジェクトのみ。マルチパッケージ構成ではない。

### MVVM (Epoxy)
- Avalonia **11.3.21** + `Epoxy.Avalonia11` 使用、`[ViewModel]` 属性でVMを宣言
- UI テーマは `FluentAvaloniaUI` (`FluentAvaloniaTheme`、アクセントカラーは `App.axaml` で定義)
- Epoxy はソースジェネレーターでコード生成するため、未ビルド状態では生成ファイルが存在しない
- **Compiled Bindings がデフォルト有効** (`AvaloniaUseCompiledBindingsByDefault=true`)
- 各 `.axaml` のルート要素に `x:DataType="vm:XXXViewModel"` が必須

### WebView
- CEF は廃止済み (CEF_Resources ディレクトリも削除済み)
- `Avalonia.Controls.WebView` の `NativeWebView` を使用 (BrowserWindow / InAppBrowserView)

### グローバル状態
`App.axaml.cs` が全ウィンドウ、Logger、Updater、WindowService を静的シングルトンで管理。全コンポーネントが `App.XXX` を直接参照する。BrowserWindow のみ初回アクセス時に遅延初期化される。

### 起動フロー
`Program.cs` → `VelopackApp.Build().Run()` → `BuildAvaloniaApp().StartWithClassicDesktopLifetime()` (DEBUG 時は `.WithDeveloperTools()`)
→ `App.Initialize()` (build.json / library.json 読み込み、Logger 初期化、Config/SearchEngine 読み込み、カルチャ適用)
→ `App.OnFrameworkInitializationCompleted()` (トレイ + 各Window作成、テーマ適用、HotKey開始、Updater開始)

### 主要ディレクトリ
| パス | 役割 |
|------|------|
| `naget/Views/` | AXAML UI定義 (`Views/Settings/`、`Views/Dialog/` 含む) |
| `naget/ViewModels/` | `[ViewModel]` 付きVMクラス |
| `naget/Models/` | 設定 (`Config/`、`Config/HotKey/`)、検索エンジンモデル |
| `naget/Helpers/` | ホットキー(SharpHook)、ロガー、Velopack更新 |
| `naget/Services/` | WindowService (ウィンドウ表示制御) |
| `naget/Common/` | Utils 等の共通ユーティリティ |
| `naget/Converter/` | 値コンバーター |
| `naget/Styling/` | 共有スタイル (`Resources.axaml`) |
| `naget/Assets/` | アイコン、ローカライズ resx (`Assets/Locales/`) |
| `naget/Setup/` | Inno Setup インストーラースクリプト (nightly/alpha) |
| `naget/Properties/PublishProfiles/` | 公開プロファイル |

### バージョン管理
- `naget/build.json` がバージョン情報のマスタ
- **埋め込みリソースとしてビルドされ、かつ NUKE `Build.cs` が実行時に上書きする**
- 書式: `{ "version": "1.0.0", "release_channel": "nightly", "release_number": "0" }`
- Build.cs が `commit_hash` / `build_date` を追記し、`release_number` 未指定時はビルド日時 (yyyyMMddHHmmss) を設定
- `full_version` は `{version}-{channel}.{number}` 形式でBuild.csが生成

## コーディング規約

- `.editorconfig` に従う。インデント: **タブ** / 改行: **CRLF** / 文字コード: **UTF-8-BOM**
- **常に明示的な型を使用 (`var` 禁止)**
- `this.` 修飾は使用しない
- インターフェースは `I` プレフィックス必須
- C# の namespace 宣言はブロックスコープ (`namespace Foo { }`) 推奨
- **コメントを追加しない** (ソースコードにコメントを書かない)
- `Nullable` 有効。Roslynator + .NET Analyzers (latest-Recommended、`Directory.Build.props` で有効化)

## プラットフォーム・更新関連

- 対応 RID: `win-x64`, `win-arm64`, `osx-arm64`
- macOS では `ShowInDock = false` (トレイ常駐アプリ)
- 更新は Velopack (`vpk` CLI) で管理、チャネル名は `{os}-{arch}-{channel}`、既定の更新 URL は `https://nagetupd.milkeyyy.com/`
- DEBUG ビルドのみ `AvaloniaUI.DiagnosticsSupport` (DevTools) を使用
- `.mcp.json` に Avalonia ドキュメント MCP が定義済み
- **`Build_env.txt` はシークレットを含むため絶対にコミット・操作しないこと**
