using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TranslateIntoChinese.Core
{
    internal static class EditorTextPicker
    {
        public const int MaxLength = 4000;

        public static string GetSelectedOrWordOrLine(ITextView view)
        {
            if (view == null) return null;

            try
            {
                var selection = view.Selection?.SelectedSpans?.FirstOrDefault() ?? default;
                if (selection.Length > 0)
                {
                    var selected = Normalize(selection.GetText());
                    if (IsValidForTranslation(selected))
                        return Truncate(selected);
                }

                var caret = view.Caret.Position.BufferPosition;
                var word = GetWordAt(caret);
                if (IsValidForTranslation(word) && word.Length >= 2)
                    return word;

                var line = caret.GetContainingLine()?.GetText();
                var normalizedLine = Normalize(line);
                if (IsValidForTranslation(normalizedLine))
                    return Truncate(normalizedLine);

                return IsValidForTranslation(word) ? word : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSelectedOrWordOrLine failed: {ex.Message}");
                return null;
            }
        }

        public static string GetWordAt(SnapshotPoint point)
        {
            try
            {
                var line = point.GetContainingLine();
                var text = line.GetText();
                if (string.IsNullOrEmpty(text)) return null;

                int offset = Math.Max(0, Math.Min(point.Position - line.Start.Position, text.Length));
                if (offset >= text.Length) offset = text.Length - 1;
                if (offset < 0) return null;

                if (!IsWordChar(text[offset]) && offset > 0 && IsWordChar(text[offset - 1]))
                    offset--;
                if (!IsWordChar(text[offset])) return null;

                int start = offset;
                while (start > 0 && IsWordChar(text[start - 1])) start--;
                int end = offset + 1;
                while (end < text.Length && IsWordChar(text[end])) end++;
                return text.Substring(start, end - start);
            }
            catch
            {
                return null;
            }
        }

        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var lines = text
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.TrimEnd())
                .ToArray();
            return string.Join("\n", lines).Trim();
        }

        public static string Truncate(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= MaxLength) return text;
            return text.Substring(0, MaxLength);
        }

        public static bool IsValidForTranslation(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            int letters = 0;
            int cjk = 0;
            foreach (var c in text)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) letters++;
                else if (c >= 0x4E00 && c <= 0x9FFF) cjk++;
            }

            if (letters == 0) return false;
            // 允许中英混排；纯中文或中文远多于英文时跳过
            if (cjk > 0 && cjk >= letters * 2) return false;
            return true;
        }

        public static bool IsValidForQuickAction(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLength) return false;
            foreach (var c in text)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) return true;
                if (c >= 0x4E00 && c <= 0x9FFF) return true;
            }
            return false;
        }

        public static bool LooksLikeSentence(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return text.IndexOf(' ') >= 0
                || text.IndexOf('\n') >= 0
                || text.Length > 25;
        }

        private static bool IsWordChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '@';
        }
    }
}
