using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeLinkAdapterExt
{
    internal static class CodeLinkParser
    {
        private static readonly Regex MarkdownLink = new Regex(
            @"^\[[^\r\n]*?\]\\?\((?<target>[^\r\n]+?)\\?\)$",
            RegexOptions.CultureInvariant);

        private static readonly Regex Location = new Regex(
            @"^(?<path>.+?)(?::(?<line>[0-9]+)(?::(?<column>[0-9]+))?|#L(?<line>[0-9]+)(?:C(?<column>[0-9]+))?)$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex AbsoluteWindowsPath = new Regex(
            @"^(?:[A-Za-z]:\\|\\\\[^\\]+\\[^\\]+\\).+",
            RegexOptions.CultureInvariant);

        private static readonly Regex InvalidPathCharacters = new Regex(
            @"[<>""|?*\x00-\x1F]",
            RegexOptions.CultureInvariant);

        public static bool TryParse(string text, out CodeLinkTarget target)
        {
            target = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string candidate = Unwrap(text.Trim());
            if (candidate.StartsWith("[", StringComparison.Ordinal))
            {
                Match markdown = MarkdownLink.Match(candidate);
                if (!markdown.Success)
                {
                    return false;
                }

                candidate = Unwrap(markdown.Groups["target"].Value.Trim());
            }

            Match location = Location.Match(candidate);
            if (!location.Success || !TryParsePosition(location.Groups["line"].Value, out int line))
            {
                return false;
            }

            int column = 1;
            if (location.Groups["column"].Success
                && !TryParsePosition(location.Groups["column"].Value, out column))
            {
                return false;
            }

            string path = location.Groups["path"].Value;
            if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                if (!Uri.TryCreate(path, UriKind.Absolute, out Uri fileUri)
                    || !fileUri.IsFile || fileUri.Query.Length != 0 || fileUri.Fragment.Length != 0)
                {
                    return false;
                }

                path = fileUri.LocalPath;
            }

            // Codex also renders Windows links with a leading slash: /C:/source/File.cs.
            if (path.Length >= 4 && path[0] == '/' && path[2] == ':' && path[3] == '/')
            {
                path = path.Substring(1);
            }

            path = path.Replace('/', '\\');
            if (!AbsoluteWindowsPath.IsMatch(path) || InvalidPathCharacters.IsMatch(path)
                || path.IndexOf(':', 2) >= 0 || path.EndsWith("\\", StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                target = new CodeLinkTarget(Path.GetFullPath(path), line, column);
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException
                || exception is NotSupportedException || exception is PathTooLongException)
            {
                return false;
            }
        }

        private static bool TryParsePosition(string value, out int position)
        {
            return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out position)
                && position > 0;
        }

        private static string Unwrap(string text)
        {
            if (text.Length >= 2)
            {
                char first = text[0];
                char last = text[text.Length - 1];
                if ((first == '"' && last == '"') || (first == '`' && last == '`')
                    || (first == '<' && last == '>'))
                {
                    return text.Substring(1, text.Length - 2).Trim();
                }
            }

            return text;
        }
    }
}
