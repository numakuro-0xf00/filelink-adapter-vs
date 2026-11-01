using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeLinkAdapterExt
{
    internal sealed class OpenCodexLinkCommand
    {
        private const int CommandId = 0x0100;
        private static readonly Guid CommandSet = new Guid("7baeb474-9137-4479-8caf-520994ecf8f2");
        private readonly AsyncPackage package;
        private readonly IVsTextManager textManager;

        private OpenCodexLinkCommand(AsyncPackage package, OleMenuCommandService commandService, IVsTextManager textManager)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.package = package;
            this.textManager = textManager;
            commandService.AddCommand(new MenuCommand(Execute, new CommandID(CommandSet, CommandId)));
        }

        public static async Task InitializeAsync(AsyncPackage package, CancellationToken cancellationToken)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var textManager = await package.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            if (commandService == null || textManager == null)
            {
                throw new InvalidOperationException("Visual Studio のコマンドまたはエディターサービスを取得できませんでした。");
            }

            new OpenCodexLinkCommand(package, commandService, textManager);
        }

        private void Execute(object sender, EventArgs eventArgs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string text;
            try
            {
                text = Clipboard.GetText(TextDataFormat.UnicodeText);
            }
            catch (ExternalException)
            {
                ShowWarning("クリップボードを読み取れませんでした。リンクをコピーし直して、もう一度実行してください。");
                return;
            }

            if (!CodeLinkParser.TryParse(text, out CodeLinkTarget target))
            {
                ShowWarning("ファイルのリンクを1件コピーしてから実行してください。\n\n"
                    + "例: [GridSession.cs (line 243)](C:/source/GridSession.cs:243)\n"
                    + "行番号と列番号は1以上の整数を指定してください。");
                return;
            }

            if (!File.Exists(target.FilePath))
            {
                ShowWarning("リンク先のファイルが見つかりません。\n\n" + target.FilePath);
                return;
            }

            try
            {
                OpenTarget(target);
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException || exception is COMException
                || exception is InvalidOperationException || exception is ArgumentException)
            {
                ShowWarning("ファイルまたは指定位置を開けませんでした。\n\n" + exception.Message);
            }
        }

        private void OpenTarget(CodeLinkTarget target)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Guid logicalView = VSConstants.LOGVIEWID_TextView;
            VsShellUtilities.OpenDocument(package, target.FilePath, logicalView,
                out IVsUIHierarchy hierarchy, out uint itemId, out IVsWindowFrame frame, out IVsTextView view);
            if (view == null)
            {
                throw new InvalidOperationException("リンク先のテキストエディターを取得できませんでした。");
            }

            ErrorHandler.ThrowOnFailure(view.GetBuffer(out IVsTextLines buffer));
            ErrorHandler.ThrowOnFailure(buffer.GetLineCount(out int lineCount));
            int line = Math.Max(0, Math.Min(target.Line - 1, lineCount - 1));
            ErrorHandler.ThrowOnFailure(buffer.GetLengthOfLine(line, out int lineLength));
            int column = Math.Min(target.Column - 1, lineLength);

            ErrorHandler.ThrowOnFailure(frame.Show());
            // This API uses character indices, so tabs do not change the requested column.
            ErrorHandler.ThrowOnFailure(textManager.NavigateToLineAndColumn(buffer, ref logicalView,
                line, column, line, column));
        }

        private void ShowWarning(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VsShellUtilities.ShowMessageBox(package, message, "Code Link Adapter",
                OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
