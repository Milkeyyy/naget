# Microsoft Store 提出用

## 提出の流れ

1. `scripts/pack-msix.ps1` を実行する
   - `naget/build.json` の `version` が `Package.appxmanifest` に反映される
   - 出力: `naget.Package\AppPackages\naget.Package_<version>_x64_arm64_bundle.msixupload`
2. Partner Center → naget → 新しい申請 → パッケージ で `.msixupload` をアップロード
3. 登録情報に下記のテキストと `Images/`、`Screenshots/` の画像を設定する
4. プロパティを設定する
   - カテゴリ: 生産性
   - プライバシー ポリシー URL: `PRIVACY_POLICY.md` を公開した URL
   - システム要件: Windows 10 version 2004 (10.0.19041) 以降 / x64・arm64
5. 年齢区分のアンケートに回答する (追加のコンテンツはありません)
6. 認定の注記に下記を貼り付ける
7. 提出する

バージョンを上げるときは `naget/build.json` の `version` を変更してから 1 を実行する。Store は同じバージョン番号のパッケージを再提出できない。

## 登録情報 (日本語)

### 説明

naget は、キーボードショートカットからすぐに検索を始められる常駐型の検索ランチャーです。

設定したショートカットキーを押すと検索ボックスが表示され、入力したキーワードを選択した検索エンジンで検索できます。検索エンジンは自由に追加・編集でき、用途ごとに別々のショートカットを割り当てられます。

検索結果はアプリ内ブラウザーでそのまま閲覧でき、閲覧履歴の確認や削除も可能です。タスクトレイに常駐するため、必要なときだけ素早く呼び出せます。

＜主な機能＞
- グローバルショートカットで検索ボックスを即表示
- 検索エンジンの追加・編集・削除
- ショートカットごとに検索エンジンを割り当て
- アプリ内ブラウザーと閲覧履歴
- ライト/ダークテーマ、日本語/英語 UI

### 短いタイトル

ショートカットひとつで、すぐ検索。

### 短い説明

ショートカットひとつで、すぐ検索。

### リリース ノート

<v1.0.0>
- 初回リリース

### 著作権と商標の情報

Copyright (C) 2026 Milkeyyy

### 開発元

Milkeyyy

## 登録情報 (English)

### Description

naget is a lightweight search launcher that lives in the notification area and opens with a keyboard shortcut.

Press your shortcut to show the search box, then search with the search engine you assigned to it. You can add and edit search engines freely and bind a different shortcut to each one.

Results open in the built-in browser, where you can review or clear your browsing history. Because naget stays in the notification area, it is there only when you need it.

<Key Features>
- Show the search box instantly with a global shortcut
- Add, edit and remove search engines
- Bind a search engine to each shortcut
- Built-in browser with browsing history
- Light and dark themes, Japanese and English UI

### Short title

Search instantly with a shortcut.

### Short description

Search instantly with a shortcut.

### Release notes

<v1.0.0>
- Initial release

### Copyright and trademark information

Copyright (C) 2026 Milkeyyy

### Developed by

Milkeyyy

## 認定の注記

naget is a tray-resident app. It does not show a main window on launch; the app runs in the notification area.
To open the app, right-click the tray icon and choose "Settings", or press a shortcut that the user has registered.
Shortcuts are not registered by default. Please open Settings from the tray icon and register a shortcut on the "Shortcut Key" tab.
The built-in browser uses the Microsoft Edge WebView2 runtime (Windows 10/11).

## スクリーンショット

`Screenshots/` に 1920x1080 の PNG を配置している。Partner Center の DesktopScreenshot にアップロードする。
現在は日本語 UI で撮影している。英語版を用意する場合はアプリの言語を English に切り替えて撮り直す。

## 画像

- `Images/StoreLogo_300x300.png` — Store ロゴ
- `Images/BoxArt_1080x1080.png` — 1:1 ボックス アート
- `Images/Poster_1920x1080.jpg` — 16:9 ポスター
- `Images/Poster_1440x1080.png` — 3:2 ポスター

## listingData CSV

`listingData_v1.0.0.csv` は Partner Center の「登録情報のインポート」用。初回の登録後に Partner Center から CSV をエクスポートし、次回以降はその CSV を元に更新する。
