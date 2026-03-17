# naget プロジェクト全体解析レポート

**日付**: 2026年3月16日
**プロジェクト**: naget (Avalonia 11 クロスプラットフォーム検索ランチャー)

---

## エグゼクティブサマリー

naget は良好な MVVM 設計と完全なローカライズ対応を備えるが、以下の重大な問題が存在:

- **async void / Task 無視による例外検出不能**
- **リソース破棄漏れ (メモリリーク)**
- **Null チェック不足 (クラッシュリスク)**
- **テスト完全欠落**
- **高度に結合されたアーキテクチャ**

---

## I. アーキテクチャ分析

### 1.1 整体構成
```
Views (AXAML)
    ↓
ViewModels (Epoxy, UI ロジック)
    ↓
Models (ConfigManager, SearchEngineManager)
    ↓
Services (WindowService)
    ↓
Helpers (Logger, HotKeyHelper, Updater)
    ↓
Common (Utils, P/Invoke)
```

### 1.2 グローバル状態管理 (App クラス)

App.axaml.cs が以下を静的シングルトンで一元管理:
- Logger, Updater, WindowService
- MainWindow, SettingsWindow, BrowserWindow (遅延初期化)
- バージョン情報, テーマ, ホットキー管理

**問題**: 全コンポーネントが App を直接参照 → テスト困難、高結合

### 1.3 主要コンポーネント

| コンポーネント | 役割 | 状態 |
|------------|------|------|
| **ConfigManager** | JSON 設定管理、HotKeyManager 統合 | ✓ 機能的だが検証なし |
| **HotKeyHelper** | SharpHook グローバルホットキー | ⚠️ スレッド安全性に懸念 |
| **Updater** | Velopack 自動更新 | ⚠️ async void, エラー処理不十分 |
| **Logger** | マルチスレッド対応ログ、GZip ローテーション | ⚠️ Dispose なし |
| **SearchEngineManager** | 検索エンジン CRUD | ⚠️ 検証/例外ハンドリング不足 |
| **BrowserWindowViewModel** | WebView (CEF) 統合 | ⚠️ イベントハンドラ未購読解除 |

---

## II. 重大な問題 (HIGH)

### ❌ 2.1 async void / Task 無視

**ファイル**:
- `naget/Helpers/Updater.cs` (L32-35)
- `naget/Helpers/HotKeyHelper.cs` (L71)

**問題**:
```csharp
// Updater.cs
public async void Start() {  // ← async void (非推奨)
    await CheckAndUpdateAttempt(silent: true);
}

// HotKeyHelper.cs
var t = hook.RunAsync();  // ← 戻り値を無視 (CS4014)
```

**影響**: 例外が上位に伝播しない → クラッシュ検出不能

**対策**: `async void` → `async Task` に変更、Task 結果を await

---

### ❌ 2.2 リソース破棄漏れ

#### Logger.StreamWriter
**ファイル**: `naget/Helpers/Logger.cs` (L48-50, 181-184, 192-212)

**問題**:
```csharp
private static StreamWriter stream;  // Dispose されない
```

**影響**: アプリ終了時にログファイルロック、データ損失の可能性

**対策**: IDisposable 実装、App.axaml.cs 終了時に `Logger.Dispose()` 呼び出し

#### BrowserWindowViewModel イベントハンドラ
**ファイル**: `naget/ViewModels/BrowserWindowViewModel.cs` (L115-116, 96)

**問題**:
```csharp
WebViewCtrl.Navigated += WebView_Navigated;           // 登録
WebViewCtrl.PropertyChanged += WebViewOnPropertyChanged;
// ウィンドウ閉じる時に -= していない
```

**影響**: メモリリーク、イベント重複実行

**対策**: `Closing` イベント (L96) で購読解除

---

### ❌ 2.3 Null チェック不足

#### MainWindowViewModel
**ファイル**: `naget/ViewModels/MainWindowViewModel.cs` (L35-36)

**問題**:
```csharp
private void DoSearch() {
    // _currentSearchEngine が null の可能性
    App.WindowService.ShowBrowser(string.Format(_currentSearchEngine.Uri, SearchWord));
}
```

**リスク**: NullReferenceException

**対策**: null チェック追加

#### AppViewModel
**ファイル**: `naget/ViewModels/AppViewModel.cs` (L37)

**問題**:
```csharp
(App.BrowserWindow.DataContext as BrowserWindowViewModel)
    .CurrentAddress = ...;  // キャスト後 null チェックなし
```

#### Updater
**ファイル**: `naget/Helpers/Updater.cs` (L88-89)

**問題**: `(Window)downloadDialog.XamlRoot` のキャスト失敗

---

### ❌ 2.4 テスト皆無

- テストプロジェクト: **0 個**
- ユニットテスト: **0 件**
- 統合テスト: **0 件**
- CI/CD (test.yml): GitHub App Token 初期化のみ、テスト処理なし

---

## III. 中程度の問題 (MEDIUM)

### ⚠️ 3.1 SOLID 原則違反

#### 単一責任原則 (SRP)
- **App.cs**: バージョン読込、ロギング初期化、ウィンドウ管理、テーマ変更、再起動を同時管理
- **ConfigManager.cs**: JSON I/O + HotKeyManager ライフサイクル + バリデーション

#### 依存性逆転原則 (DIP)
- 具象クラス直接参照 (HotKeyHelper, Logger, Updater)
- インターフェース層なし → テスト困難

#### 開放・閉鎖原則 (OCP)
- 新しいホットキーアクション追加時に `HotKeyAction.cs` 直接修正が必要
- 新しい検索エンジンタイプ追加時にコード修正が必要

### ⚠️ 3.2 エラーハンドリング不足

| コンポーネント | 問題 | 影響 |
|------------|------|------|
| **ConfigManager** (L74) | JSON デシリアライズ失敗時に新規作成 | データ損失 |
| **Updater** (L73-80) | ダウンロード失敗時のリトライなし | 更新失敗をサイレント処理 |
| **SearchEngineManager** (L114-127) | Delete 失敗時に KeyNotFoundException | 呼び出し側で未ハンドル |
| **Logger.CompressLogFile()** | 例外ハンドリングなし | GZip 処理失敗時にクラッシュ |

### ⚠️ 3.3 入力検証なし

#### URI バリデーション不足
**ファイル**: `naget/Models/SearchEngine/SearchEngineManager.cs` (L104-106)

```csharp
public static void Create(string name, string uri) {
    _engineList.List.Add(new SearchEngineClass(name, uri));
    // uri の検証なし → 無効/悪意のある URL が保存可能
}
```

#### JSON スキーマ検証なし
**ファイル**: `naget/Models/Config/ConfigManager.cs` (L74)

```csharp
_configBase = JsonSerializer.Deserialize<ConfigBaseClass>(
    File.ReadAllText(FilePath)) ?? new();
// スキーマ検証なし → 破損ファイルで予期せぬ動作
```

### ⚠️ 3.4 スレッド安全性の懸念

**HotKeyHelper.cs** (L26, 84-160)

```csharp
private static KeyboardKeyEventArgs currentModifiers;  // 複数スレッドからアクセス
```

- バックグラウンドスレッド (SharpHook) + UI スレッド からアクセス
- ロック保護が `pressedKeysLock` のみで不十分

**Logger.cs** (L151-164)

- lock 内から `CreateLogfile()` 呼び出し時の競合状態の可能性

### ⚠️ 3.5 コード重複

#### SearchViewModel.axaml
```csharp
// L68-110: Create ダイアログ
// L145-192: Edit ダイアログ
// ContentDialog 作成コードが重複 → ファクトリメソッド化推奨
```

#### BrowserWindowViewModel
```csharp
// L225-250: OnCurrentAddressChangedAsync()
// L219-223: WebViewOnPropertyChanged()
// L215-217: WebView_Navigated()
// 同じロジックを複数箇所で重複実行
```

#### HotKeyClass.cs
```csharp
// L103-275: 150+ キーの巨大辞書がハードコード
// 外部化/リソース化推奨
```

### ⚠️ 3.6 Dead Code

- **HotKeyAction.cs**: コメント化された古いコード
- **BrowserWindowViewModel.cs** (L177-191): コメント化されたコード
- **ConfigClass.cs** (L117-137): コメント化された古い `BrowserWindowConfig`

---

## IV. ビルド・CI/CD の問題

### 4.1 SDK バージョン不統一

**global.json**:
```json
{
  "sdk": {
    "version": "10.0.104",
    "rollForward": "latestFeature"  // ⚠️ 予期しない SDK 変更の可能性
  }
}
```

### 4.2 ワークフロー間でバージョン異なる

- `build-nightly.yml`: .NET 10.0.x
- `build-alpha.yml`: .NET 8.0.x  ← **不統一**

### 4.3 WebViewControl アーキテクチャ差

- x64: 3.120.11
- arm64: 3.120.10  ← **バージョン不一致**

---

## V. UI/UX の問題

### 5.1 アクセシビリティ (LOW)

- すべての AXAML で `AutomationId` 完全欠落 → スクリーンリーダー非対応
- `AccessKey` (メニュー項目キーボードショートカット) なし
- `TabIndex` 定義なし

### 5.2 スタイリング問題

**Resources.axaml**: 実質空、共有スタイル未定義

**App.xaml**: フォント定義がコメント化済み (L37-39)

**AppSettingsView.axaml** (L44):
```xaml
Foreground="{DynamicResource ThemeForegroundBrush}"
<!-- ThemeForegroundBrush が未定義 -->
```

### 5.3 バインディング型不一致

**ShortcutKeyView.axaml** (L44):
```xaml
IsVisible="{Binding HotKeyPresetList.Count}"
<!-- int → bool 暗黙変換 (非標準) -->
```

### 5.4 レスポンシブ対応不足

- MainWindow `MinWidth="800"` 固定 → 小画面非対応
- DPI スケーリング未考慮

---

## VI. セキュリティ考慮事項

| 問題 | 重要度 | 説明 |
|------|--------|------|
| P/Invoke 検証なし | 中 | user32.dll SetForegroundWindow, ShowWindow → 入力制御なし |
| JSON デシリアライズ | 中 | スキーマ検証なし |
| URL 検証なし | 中 | SearchEngine URI に悪意的 URL 可能 |
| グローバル状態アクセス | 中 | App 静的フィールド → どこからでも変更可能 |
| ログ情報保持 | 低 | 最大30日間センシティブ情報を保存 |

---

## VII. 良い点 ✅

- **ローカライズ完全実装**: ja-JP, en-US / 398 文字列
- **MVVM パターン**: 全般的に正しく実装
- **Compiled Bindings**: 有効 (`AvaloniaUseCompiledBindingsByDefault=true`)
- **ログローテーション**: 10MB / GZip 圧縮 / 30日自動削除
- **Velopack チャネル管理**: OS/アーキテクチャ/チャネル別に整理
- **BrowserWindow Lazy Loading**: macOS IME フリーズ対策
- **コーディング規約**: .editorconfig に従う (タブ, CRLF, UTF-8-BOM)

---

## VIII. 推奨改善 (優先度順)

### HIGH (即時)

1. **async void → async Task** (`Updater.cs`, `HotKeyHelper.cs`)
   - 例外ハンドリング強化

2. **リソース Dispose 実装** (`Logger.cs`, `BrowserWindowViewModel.cs`)
   - メモリリーク防止

3. **Null チェック追加** (`MainWindowViewModel.cs`, `AppViewModel.cs`)
   - NullReferenceException 防止

4. **テストプロジェクト作成** (xUnit, Moq)
   - ConfigManager, SearchEngineManager 対象

### MEDIUM

5. **DI 導入** (`Microsoft.Extensions.DependencyInjection`)
   - App 静的シングルトン解消

6. **エラーハンドリング強化**
   - ConfigManager, Updater, SearchEngineManager

7. **URI バリデーション追加**
   - SearchEngineManager.Create()

8. **CI/CD バージョン統一**
   - build-alpha.yml を .NET 10.0.x に統一

9. **Dead code 削除**
   - HotKeyAction.cs, BrowserWindowViewModel.cs, ConfigClass.cs

### LOW

10. **AutomationId 追加** (全 AXAML)
11. **共有スタイル定義** (`Resources.axaml`)
12. **ItemsControl 仮想化** (`SearchView.axaml`)

---

## IX. 評価サマリー

| 範囲 | 評価 | 主要懸念 |
|------|------|--------|
| **アーキテクチャ** | ⭐⭐ | 高結合, SOLID 違反, テスト不可 |
| **コード品質** | ⭐⭐⭐ | スタイル良好, 例外/検証不足 |
| **エラー処理** | ⭐⭐ | 制限的, リカバリなし |
| **テスト** | ⭐ | 完全欠落 |
| **セキュリティ** | ⭐⭐ | 検証なし, グローバル状態乱用 |
| **パフォーマンス** | ⭐⭐⭐ | 一般的, ログ I/O 競合の可能性 |
| **ローカライズ** | ⭐⭐⭐⭐⭐ | 完全実装 |
| **MVVM 設計** | ⭐⭐⭐⭐ | 全般良好 |

---

## 附録: ファイル構成

```
v:/naget/
├── naget/
│   ├── App.axaml / App.axaml.cs
│   ├── Program.cs
│   ├── naget.csproj
│   ├── build.json
│   ├── Common/Utils.cs
│   ├── Models/
│   │   ├── Config/ (ConfigManager, ConfigClass, HotKey/)
│   │   └── SearchEngine/ (SearchEngineManager, SearchEngineClass)
│   ├── ViewModels/
│   │   ├── AppViewModel.cs
│   │   ├── MainWindowViewModel.cs
│   │   ├── BrowserWindowViewModel.cs
│   │   └── Settings/ (ShortcutKeyViewModel, SearchViewModel, AppSettingsViewModel)
│   ├── Views/
│   │   ├── MainWindow.axaml / .axaml.cs
│   │   ├── BrowserWindow.axaml / .axaml.cs
│   │   ├── SettingsWindow.axaml / .axaml.cs
│   │   ├── AboutWindow.axaml
│   │   ├── UpdateCompleteWindow.axaml
│   │   └── Settings/ (各設定画面)
│   ├── Helpers/
│   │   ├── Logger.cs (マルチスレッド対応, ローテーション)
│   │   ├── HotKeyHelper.cs (SharpHook)
│   │   └── Updater.cs (Velopack)
│   ├── Services/WindowService.cs
│   ├── Converter/
│   │   ├── HotKeyActionNameConverter.cs
│   │   └── ThemeNameConverter.cs
│   ├── Assets/
│   │   ├── Locales/ (Resources.resx, Resources.ja-JP.resx)
│   │   ├── Fonts/ (Noto Sans JP, 9 ウェイト)
│   │   └── Icon.ico
│   ├── Styling/Resources.axaml
│   └── CEF_Resources/ (win-x64, win-arm64)
├── build/ (_build.csproj - NUKE)
├── .github/workflows/
│   ├── build-nightly.yml
│   ├── build-alpha.yml
│   └── test.yml
├── .editorconfig
├── Directory.Build.props
├── global.json
└── naget.sln
```

---

**報告者**: Claude Code Agent
**実施日**: 2026-03-16
