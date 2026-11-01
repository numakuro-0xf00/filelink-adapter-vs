# filelink-adapter-vs (Code Link Adapter)

CodexアプリからコピーしたファイルリンクをVisual Studioで開き、指定行へカーソルを移動するVSIX拡張です。ソリューションに含まれないファイルも開けます。

## 使い方

1. Codexから、次のようなリンクを1件コピーします。

   ```text
   [GridSession.cs (line 243)]\(C:/Users/home/source/repos/hyakushiki-kannon/hyakushiki-kannon.Core/GridSession.cs:243)
   ```

2. Visual Studioで［ツール］→［Codexリンクをクリップボードから開く］を実行します。
3. リンク先のファイルが開き、243行目の先頭へ移動します。

［ツール］→［オプション］→［環境］→［キーボード］で `OpenCodexLinkFromClipboard` を検索すると、任意のショートカットを割り当てられます。

## 対応形式

```text
[File.cs (line 12)](C:/source/File.cs:12)
[File.cs (line 12)]\(C:/source/File.cs:12)
C:\source\File.cs:12
C:/source/File.cs:12:8
C:/source/File.cs#L12C8
file:///C:/my%20project/File.cs:12
```

Windowsの絶対パスとUNCパスに対応します。行・列は1から数え、列省略時は行頭へ移動します。行・列がファイルの末尾を超える場合は、最終行・行末に収めます。表示名ではなく、リンク先に記載されたパスと位置を使います。

空のクリップボード、複数リンク、不正な位置、存在しないファイルはメッセージで知らせます。Webリンクや相対パスには対応していません。

## ビルドとインストール

Windows、Visual Studio 2022 17.14以降の拡張機能開発環境、および.NET Framework 4.8 targeting packを使用します。Visual Studio Developer PowerShellで実行します。

```powershell
msbuild CodeLinkAdapterExt\CodeLinkAdapterExt.csproj /restore /p:Configuration=Debug
msbuild CodeLinkAdapterExt\CodeLinkAdapterExt.csproj /restore /p:Configuration=Release
```

生成物は `CodeLinkAdapterExt/bin/Release/net48/CodeLinkAdapterExt.vsix` です。VSIXを開いて対象のVisual Studioにインストールし、Visual Studioを再起動します。

開発中の確認には実験用インスタンスを使用してください。Visual Studio Developer PowerShellで、DebugのVSIXを実験用インスタンスへインストールしてから起動します。

```powershell
VSIXInstaller.exe /rootSuffix:Exp CodeLinkAdapterExt\bin\Debug\net48\CodeLinkAdapterExt.vsix
devenv /RootSuffix Exp
```

## テスト

Windows、.NET SDK、および.NET Framework 4.8の環境で実行します。

```powershell
dotnet test CodeLinkAdapterExt.Tests\CodeLinkAdapterExt.Tests.csproj
```

MSTestで、提示されたリンク形式、行・列、空白・日本語・括弧を含むパス、UNC、file URI、不正入力を検証します。テストは解析処理のソースを共有し、Visual Studioを起動せずに実行します。

インストール後は、通常のリンク、開いているファイル、ソリューション外のファイル、範囲外の行・列、および存在しないパスを手動確認してください。
