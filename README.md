# filelink-adapter-vs (Code Link Adapter)

CodexアプリからコピーしたファイルリンクをVisual Studioで開き、指定行へカーソルを移動するVSIX拡張です。ソリューションに含まれないファイルも開けます。

## 導入手順

### 必要な環境

- Windows 64ビット
- Visual Studio 2022 17.14以降（amd64）
- .NET Framework 4.8

以下はソースからVSIXを作成して導入する手順です。ビルド用にGitと.NET SDKを用意し、Visual Studio Installerで［Visual Studio拡張機能の開発］ワークロードと.NET Framework 4.8 targeting packを追加してください。ワークロードの追加方法は[Microsoftの公式手順](https://learn.microsoft.com/en-us/visualstudio/extensibility/installing-the-visual-studio-sdk?view=visualstudio)を参照してください。

### 1. ソースを取得してVSIXを作成する

Visual Studioに付属するDeveloper PowerShellを開き、次を実行します。

```powershell
git clone https://github.com/numakuro-0xf00/filelink-adapter-vs.git
cd filelink-adapter-vs
msbuild CodeLinkAdapterExt\CodeLinkAdapterExt.csproj /restore /p:Configuration=Release
```

初回はNuGetパッケージを復元します。成功すると、リポジトリのルートから次の場所にインストール用VSIXが生成されます。

```text
CodeLinkAdapterExt/bin/Release/net48/CodeLinkAdapterExt.vsix
```

### 2. Visual Studioにインストールする

1. 編集中のファイルを保存し、Visual Studioを終了します。
2. 生成した `CodeLinkAdapterExt.vsix` をエクスプローラーでダブルクリックします。
3. VSIX Installerで導入先のVisual Studioを選び、［インストール］を押します。
4. 完了後、導入先のVisual Studioを起動します。
5. ［拡張機能］→［拡張機能の管理］で「Code Link Adapter」がインストール済みになっていることを確認します。
6. ［ツール］メニューに［Codexリンクをクリップボードから開く］が表示されれば、使用を開始できます。

VSIXのインストールや拡張機能の管理については[Microsoftの公式手順](https://learn.microsoft.com/en-us/visualstudio/ide/finding-and-using-visual-studio-extensions?view=visualstudio)も参照してください。

## 使用方法

### リンク先のファイルと行を開く

1. Codexの回答から、ファイルパスと行番号を含むリンクを1件コピーします。表示名の `GridSession.cs (line 243)` だけでなく、次のようにリンク先を含む文字列をコピーしてください。

   ```text
   [GridSession.cs (line 243)]\(C:/source/GridSession.cs:243)
   ```

   次のようなパスと行番号だけの文字列でも使用できます。例のパスは、自分の環境に実在するファイルへ置き換えてください。

   ```text
   C:/source/GridSession.cs:243
   ```

2. 移動先にするVisual Studioのウィンドウをアクティブにします。
3. ［ツール］→［Codexリンクをクリップボードから開く］を実行します。
4. `GridSession.cs` が開き、243行目の先頭へカーソルが移動します。既に開いているファイルも指定位置へ移動します。

列も指定する場合は、`C:/source/GridSession.cs:243:8` のように末尾へ列番号を付けます。この例では243行目の8文字目へ移動します。行・列は1から数え、列を省略すると行頭へ移動します。

リンク先のファイルはソリューションに含まれていなくても開けます。表示名ではなくリンク先のパスと位置を使い、行・列がファイルの末尾を超える場合は最終行・行末に収めます。

### キーボードショートカットを設定する

既定のショートカットは **Ctrl+Alt+K → Ctrl+Alt+L** です。リンクをコピーし、Visual StudioでCtrlとAltを押しながらK、続いてLを押すと、指定ファイル・行へ移動します。拡張の更新後はVSIXを再インストールし、Visual Studioを再起動してください。

キーを変更する場合や既存の割り当てと競合する場合は、次の手順で設定します。

1. ［ツール］→［オプション］→［環境］→［キーボード］を開きます。
2. コマンド一覧で `OpenCodexLinkFromClipboard` を検索して選択します。
3. 希望するキーを入力し、既存コマンドとの重複を確認して［割り当て］→［OK］を押します。
4. 次回からはリンクをコピーし、Visual Studioで設定したキーを押すと移動できます。

## 対応リンク形式

```text
[File.cs (line 12)](C:/source/File.cs:12)
[File.cs (line 12)]\(C:/source/File.cs:12)
C:\source\File.cs:12
C:/source/File.cs:12:8
C:/source/File.cs#L12C8
file:///C:/my%20project/File.cs:12
\\server\share\File.cs:12
```

Windowsの絶対パスとUNCパスに対応します。空白・日本語・括弧を含むパスも使用できます。`file:///` 形式では `%20` などのURLエンコードを復元します。

空のクリップボード、複数リンク、不正な位置、存在しないファイルはメッセージで知らせます。Webリンクや相対パスには対応していません。

## うまく動かない場合

| 症状 | 確認すること |
| --- | --- |
| ［ツール］にコマンドが表示されない | 導入先のVisual Studioを起動しているか、「Code Link Adapter」が有効かを確認し、Visual Studioを再起動してください。 |
| 「ファイルのリンクを1件コピーしてから実行してください」と表示される | 表示名だけでなく、絶対パスと行番号を含むリンクを1件コピーしてください。行・列には1以上の整数を使います。 |
| 「リンク先のファイルが見つかりません」と表示される | コピーしたパスが、このPC上に存在するファイルを指しているか確認してください。 |
| `msbuild` が見つからない | Visual StudioのDeveloper PowerShellを使い、拡張機能開発用ワークロードが追加されているか確認してください。 |

## 開発・テスト

取得済みのリポジトリのルートで実行します。

```powershell
msbuild CodeLinkAdapterExt\CodeLinkAdapterExt.csproj /restore /p:Configuration=Debug
dotnet test CodeLinkAdapterExt.Tests\CodeLinkAdapterExt.Tests.csproj
```

MSTestで、コピーしたリンク形式、行・列、空白・日本語・括弧を含むパス、UNC、file URI、不正入力を検証します。テストは解析処理のソースを共有し、Visual Studioを起動せずに実行します。

開発中の確認には実験用インスタンスを使用してください。Visual Studio Developer PowerShellで、DebugのVSIXを実験用インスタンスへインストールしてから起動します。

```powershell
VSIXInstaller.exe /rootSuffix:Exp CodeLinkAdapterExt\bin\Debug\net48\CodeLinkAdapterExt.vsix
devenv /RootSuffix Exp
```

インストール後は、通常のリンク、開いているファイル、ソリューション外のファイル、範囲外の行・列、および存在しないパスを手動確認してください。
