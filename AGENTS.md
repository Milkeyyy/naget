# AGENTS.md

このファイルは OpenCode がリポジトリで作業する際の案内です。

## ビルド・実行

- .NET **10.0** ターゲット (`global.json`: 10.0.104, csproj: `net10.0`)
- 開発ビルド: `dotnet build naget/naget.csproj`
- 製品ビルドは NUKE 経由: `./build.cmd --runtime win-x64 --releasechannel nightly --releasenumber <番号>`
- NUKE のエントリポイントは `build/Build.cs`、既定ターゲットは `Compile` (Restore → `UpdateLibraryList` → `dotnet publish`、framework-dependent + PublishTrimmed)
- `UpdateLibraryList` が `naget/library.json` を `nuget-license` で再生成する。ツールは `.config/dotnet-tools.json` (3.1.4) で固定し、`dotnet tool restore` もターゲット内で実行。初回はネットワークが必要で、失敗時は既存の一覧を残してビルドを継続する
- ライセンス一覧の対象は `naget/naget.csproj`。`scripts/custom.json` で特定パッケージの除外・差し替えができる
- ビルド出力は `_Pack/{runtime}/Build/`
- リリース作業 (vpk パッキングと GitHub Releases アップロード) は `Build_win-x64_nightly.bat` / `Build_win-arm64_nightly.bat` / `Build_osx-arm64_nightly.sh` を使用。CI は `.github/workflows/build-nightly.yml`
- Microsoft Store 用パッケージは `scripts/pack-msix.ps1` で作成する (`dotnet` CLI 不可、Visual Studio の MSBuild が必要)
- Alpha リリースは `alpha` ブランチで管理する。`dev` から `alpha` へ PR を作成してマージし、GitHub Actions の `Build Alpha` を手動実行 (`release_number` に `1` など) すると `v1.0.0-alpha.N` の draft release が作成される。公開は手動で行う
- `dev -> alpha` の PR では `Build Check` (`dotnet build`) のみ実行される
- **テストプロジェクトは存在しない**。テストコマンドを実行しないこと

## アーキテクチャ

### 単一プロジェクト
`sln` に `naget`、`build/_build.csproj`、`naget.Package` (MSIX 用 wapproj) の3プロジェクト。マルチパッケージ構成ではない。

### MVVM (Epoxy)
- Avalonia **11.3.22** + `Epoxy.Avalonia11` 使用、`[ViewModel]` 属性でVMを宣言
- UI テーマは `FluentAvaloniaUI` (`FluentAvaloniaTheme`、アクセントカラーは `App.axaml` で定義)
- Epoxy はソースジェネレーターでコード生成するため、未ビルド状態では生成ファイルが存在しない
- **Compiled Bindings がデフォルト有効** (`AvaloniaUseCompiledBindingsByDefault=true`)
- 各 `.axaml` のルート要素に `x:DataType="vm:XXXViewModel"` が必須

### WebView
- CEF は廃止済み (CEF_Resources ディレクトリも削除済み)
- `Avalonia.Controls.WebView` の `NativeWebView` を使用 (BrowserWindow / InAppBrowserView)

### ライセンス一覧
- `naget/library.json` はビルド時に生成され、埋め込みリソースとして同梱される
- `App.Libraries` (`List<LibraryInfo>`) を `AboutWindow` のリンクから開く `LicenseWindow` で一覧表示する
- 各行のライセンス名は `HyperlinkButton` (`NavigateUri`) で `LicenseUrl` を既定ブラウザーで開く

### グローバル状態
`App.axaml.cs` が全ウィンドウ、Logger、Updater、WindowService を静的シングルトンで管理。全コンポーネントが `App.XXX` を直接参照する。BrowserWindow のみ初回アクセス時に遅延初期化される。

### 起動フロー
`Program.cs` → `VelopackApp.Build().Run()` → `BuildAvaloniaApp().StartWithClassicDesktopLifetime()` (DEBUG 時は `.WithDeveloperTools()`)
→ `App.Initialize()` (build.json / library.json 読み込み、Logger 初期化、Config/SearchEngine 読み込み、カルチャ適用)
→ `App.OnFrameworkInitializationCompleted()` (トレイ + 各Window作成、テーマ適用、HotKey開始、Updater開始)

### Microsoft Store (MSIX)
- `naget.Package` は `net10.0` のアプリを参照する Windows Application Packaging Project。パッケージ内は self-contained (約150MB/アーキテクチャ) で、x64 + arm64 の msixbundle を `.msixupload` にまとめる
- `scripts/pack-msix.ps1` が `naget/build.json` の `version` を `Package.appxmanifest` に `x.y.z.0` で反映し、`UapAppxPackageBuildMode=StoreUpload` でビルドする。`build.json` はビルド中だけ `release_channel=store` になり、終了後に元へ戻る
- Store 版は Velopack を使わない。`PackageHelper.IsPackaged()` (`GetCurrentPackageFullName`) で判定して更新確認をスキップし、トレイの「アップデートを確認」は `ms-windows-store://pdp/?PFN=...` を開く
- 再起動は `shell:AppsFolder\<PFN>!App` 経由 (`naget.exe` の直接起動はパッケージ ID を失うため)
- 提出手順・掲載文・画像は `Store/`。提出のたびに `naget/build.json` の `version` を上げる
- パッケージ言語は en-US / ja-JP。`naget.Package` のダミー `.resw` 生成タスクで登録している

### 主要ディレクトリ
| パス | 役割 |
|------|------|
| `naget/Views/` | AXAML UI定義 (`Views/Settings/`、`Views/Dialog/` 含む) |
| `naget/ViewModels/` | `[ViewModel]` 付きVMクラス |
| `naget/Models/` | 設定 (`Config/`、`Config/HotKey/`)、検索エンジン、`LibraryInfo` |
| `naget/Helpers/` | ホットキー(SharpHook)、ロガー、Velopack更新 |
| `naget/Services/` | WindowService (ウィンドウ表示制御) |
| `naget/Common/` | Utils 等の共通ユーティリティ |
| `naget/Converter/` | 値コンバーター |
| `naget/Styling/` | 共有スタイル (`Resources.axaml`) |
| `naget/Assets/` | アイコン、ローカライズ resx (`Assets/Locales/`) |
| `naget/Setup/` | Inno Setup インストーラースクリプト (nightly/alpha) |
| `naget/Properties/PublishProfiles/` | 公開プロファイル |
| `naget.Package/` | MSIX 用 WAP (wapproj、AppxManifest、タイル画像) |
| `Store/` | Microsoft Store 提出用の画像・掲載文・CSV・プライバシーポリシー |
| `scripts/pack-msix.ps1` | Store 用 `.msixupload` のビルドスクリプト |

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
