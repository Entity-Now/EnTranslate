using EnvDTE;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Text;
using System.Threading;
using System.Windows.Input;
using TranslateIntoChinese.Model;

namespace TranslateIntoChinese.Core
{
    public sealed class HotkeySpec
    {
        public ModifierKeys FirstModifiers { get; set; }
        public Key FirstKey { get; set; }
        public bool HasChord { get; set; }
        public ModifierKeys SecondModifiers { get; set; }
        public Key SecondKey { get; set; }
        public string Display { get; set; }
    }

    /// <summary>
    /// 解析 / 匹配用户自定义快捷键，并尽量同步到 VS 命令绑定。
    /// 支持单键（Ctrl+Alt+T）和组合键（Ctrl+K, K）。
    /// </summary>
    public static class HotkeyService
    {
        public static HotkeySpec Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Parse("Ctrl+Alt+T");

            var parts = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return Parse("Ctrl+Alt+T");

            var first = ParseChord(parts[0]);
            if (first.Key == Key.None) return Parse("Ctrl+Alt+T");

            var spec = new HotkeySpec
            {
                FirstModifiers = first.Modifiers,
                FirstKey = first.Key
            };

            if (parts.Length > 1)
            {
                var second = ParseChord(parts[1]);
                if (second.Key != Key.None)
                {
                    spec.HasChord = true;
                    spec.SecondModifiers = second.Modifiers;
                    spec.SecondKey = second.Key;
                }
            }

            spec.Display = Format(spec);
            return spec;
        }

        public static string Format(HotkeySpec spec)
        {
            if (spec == null) return "Ctrl+Alt+T";
            var first = FormatChord(spec.FirstModifiers, spec.FirstKey);
            if (!spec.HasChord) return first;
            return first + ", " + FormatChord(spec.SecondModifiers, spec.SecondKey);
        }

        public static string Capture(KeyEventArgs e)
        {
            if (e == null) return null;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifier(key)) return null;

            var mods = Keyboard.Modifiers;
            return FormatChord(mods, key);
        }

        public static bool Matches(HotkeySpec spec, KeyEventArgs e)
        {
            if (spec == null || e == null) return false;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifier(key)) return false;
            return SameChord(spec.FirstModifiers, spec.FirstKey, Keyboard.Modifiers, key);
        }

        public static bool MatchesSecond(HotkeySpec spec, KeyEventArgs e)
        {
            if (spec == null || !spec.HasChord || e == null) return false;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifier(key)) return false;
            var mods = Keyboard.Modifiers;
            // Ctrl+K, K 与 Ctrl+K, Ctrl+K 都接受
            return SameChord(spec.SecondModifiers, spec.SecondKey, mods, key)
                || SameChord(spec.FirstModifiers, spec.SecondKey, mods, key)
                || (spec.SecondModifiers == ModifierKeys.None && key == spec.SecondKey);
        }

        public static async Task ApplyAsync(string gesture = null)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var spec = Parse(gesture ?? TranslateIntoChinese.Model.Constants.Config?.TranslateHotkey);
                var binding = spec.Display;
                if (string.IsNullOrWhiteSpace(binding)) return;

                var dte = await VS.GetServiceAsync<DTE, DTE>();
                if (dte?.Commands == null) return;

                Command cmd = FindCommand(dte);
                if (cmd == null) return;

                cmd.Bindings = new object[]
                {
                    "Global::" + binding,
                    "Text Editor::" + binding
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HotkeyService.ApplyAsync failed: {ex.Message}");
            }
        }

        private static Command FindCommand(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                foreach (Command command in dte.Commands)
                {
                    try
                    {
                        if (command == null || command.ID != PackageIds.TranslateSelectionCommand) continue;
                        if (!Guid.TryParse(command.Guid, out var guid)) continue;
                        if (guid == PackageGuids.TranslateIntoChinese) return command;
                    }
                    catch
                    {
                        // 个别命令访问 GUID 会抛，跳过即可
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindCommand failed: {ex.Message}");
            }
            return null;
        }

        private static (ModifierKeys Modifiers, Key Key) ParseChord(string text)
        {
            var mods = ModifierKeys.None;
            var key = Key.None;
            foreach (var raw in (text ?? string.Empty).Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var token = raw.Trim();
                if (token.Length == 0) continue;
                if (token.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                    token.Equals("Control", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= ModifierKeys.Control;
                    continue;
                }
                if (token.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= ModifierKeys.Alt;
                    continue;
                }
                if (token.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= ModifierKeys.Shift;
                    continue;
                }
                if (token.Equals("Win", StringComparison.OrdinalIgnoreCase) ||
                    token.Equals("Windows", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= ModifierKeys.Windows;
                    continue;
                }

                if (Enum.TryParse(token, true, out Key parsed))
                    key = parsed;
                else if (token.Length == 1 && Enum.TryParse(token.ToUpperInvariant(), out parsed))
                    key = parsed;
            }
            return (mods, key);
        }

        private static string FormatChord(ModifierKeys mods, Key key)
        {
            var sb = new StringBuilder();
            if (mods.HasFlag(ModifierKeys.Control)) sb.Append("Ctrl+");
            if (mods.HasFlag(ModifierKeys.Alt)) sb.Append("Alt+");
            if (mods.HasFlag(ModifierKeys.Shift)) sb.Append("Shift+");
            if (mods.HasFlag(ModifierKeys.Windows)) sb.Append("Win+");
            sb.Append(key == Key.None ? "?" : key.ToString());
            return sb.ToString();
        }

        private static bool SameChord(ModifierKeys expectedMods, Key expectedKey, ModifierKeys actualMods, Key actualKey)
        {
            return expectedKey == actualKey && expectedMods == actualMods;
        }

        private static bool IsModifier(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LeftShift || key == Key.RightShift
                || key == Key.LWin || key == Key.RWin
                || key == Key.System;
        }
    }

    internal sealed class TextViewHotkeyFilter
    {
        private readonly IWpfTextView _view;
        private HotkeySpec _pendingChord;
        private DateTime _chordUntil;

        public TextViewHotkeyFilter(IWpfTextView view)
        {
            _view = view;
            _view.VisualElement.PreviewKeyDown += OnPreviewKeyDown;
            _view.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _view.VisualElement.PreviewKeyDown -= OnPreviewKeyDown;
            _view.Closed -= OnClosed;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var spec = HotkeyService.Parse(TranslateIntoChinese.Model.Constants.Config?.TranslateHotkey);
                if (spec == null) return;

                if (_pendingChord != null && DateTime.UtcNow <= _chordUntil)
                {
                    if (HotkeyService.MatchesSecond(_pendingChord, e))
                    {
                        e.Handled = true;
                        _pendingChord = null;
                        FireTranslate();
                    }
                    else if (!IsModifierKey(e))
                    {
                        _pendingChord = null;
                    }
                    return;
                }

                if (!HotkeyService.Matches(spec, e)) return;

                if (spec.HasChord)
                {
                    _pendingChord = spec;
                    _chordUntil = DateTime.UtcNow.AddSeconds(1.5);
                    e.Handled = true;
                    return;
                }

                e.Handled = true;
                FireTranslate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TextViewHotkeyFilter failed: {ex.Message}");
            }
        }

        private void FireTranslate()
        {
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await TranslateSelectionCommand.ExecuteFromHotkeyAsync(_view);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FireTranslate failed: {ex.Message}");
                }
            });
        }

        private static bool IsModifierKey(KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            return key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LeftShift || key == Key.RightShift
                || key == Key.LWin || key == Key.RWin
                || key == Key.System;
        }
    }

    internal static class TranslateCommandGuard
    {
        private static long _lastTicks;

        public static bool TryEnter()
        {
            var now = DateTime.UtcNow.Ticks;
            if (now - Interlocked.Read(ref _lastTicks) < TimeSpan.FromMilliseconds(350).Ticks)
                return false;
            Interlocked.Exchange(ref _lastTicks, now);
            return true;
        }
    }
}
