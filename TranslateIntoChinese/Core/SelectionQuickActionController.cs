using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TranslateIntoChinese.Model;
using TranslateIntoChinese.View;

namespace TranslateIntoChinese.Core
{
    internal sealed class SelectionQuickActionController
    {
        public const string LayerName = "QuickActionAdornmentLayer";

        private readonly IWpfTextView _view;
        private readonly DispatcherTimer _debounce;
        private QuickActionBar _bar;
        private SnapshotSpan _currentSpan;
        private bool _busy;

        public SelectionQuickActionController(IWpfTextView view)
        {
            _view = view;
            _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(220) };
            _debounce.Tick += Debounce_Tick;

            _view.Selection.SelectionChanged += Selection_SelectionChanged;
            _view.LayoutChanged += View_LayoutChanged;
            _view.Closed += View_Closed;
            _view.VisualElement.PreviewKeyDown += VisualElement_PreviewKeyDown;
        }

        private void View_Closed(object sender, EventArgs e)
        {
            _debounce.Stop();
            _debounce.Tick -= Debounce_Tick;
            _view.Selection.SelectionChanged -= Selection_SelectionChanged;
            _view.LayoutChanged -= View_LayoutChanged;
            _view.Closed -= View_Closed;
            _view.VisualElement.PreviewKeyDown -= VisualElement_PreviewKeyDown;
            Hide();
        }

        private void Selection_SelectionChanged(object sender, EventArgs e)
        {
            if (_busy) return;
            _debounce.Stop();
            _debounce.Start();
        }

        private void View_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (_bar == null || _busy) return;
            PositionBar();
        }

        private void VisualElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Hide();
        }

        private void Debounce_Tick(object sender, EventArgs e)
        {
            _debounce.Stop();
            try
            {
                Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QuickAction refresh failed: {ex.Message}");
                Hide();
            }
        }

        private void Refresh()
        {
            if (_busy) return;
            if (!ShouldShow(out var span))
            {
                Hide();
                return;
            }

            _currentSpan = span;
            EnsureBar();
            PositionBar();
        }

        private bool ShouldShow(out SnapshotSpan span)
        {
            span = default;
            var cfg = Constants.Config;
            if (cfg == null || !cfg.UseAiTranslate || !cfg.EnableQuickActions) return false;
            if (_view.IsClosed || _view.Selection == null) return false;
            if (_view.Selection.IsEmpty || _view.Selection.Mode != TextSelectionMode.Stream) return false;
            if (Mouse.LeftButton == MouseButtonState.Pressed) return false;

            var selected = _view.Selection.SelectedSpans.FirstOrDefault();
            if (selected.Length == 0 || selected.Length > EditorTextPicker.MaxLength) return false;

            var text = selected.GetText();
            if (!EditorTextPicker.IsValidForQuickAction(text)) return false;

            var actions = BuiltInQuickActions.Merge(cfg.QuickActions).Where(a => a.Enabled).ToList();
            if (actions.Count == 0) return false;

            span = selected;
            return true;
        }

        private void EnsureBar()
        {
            var actions = BuiltInQuickActions.Merge(Constants.Config.QuickActions)
                .Where(a => a.Enabled)
                .ToList();

            if (_bar == null)
            {
                _bar = new QuickActionBar();
                _bar.ActionRequested += Bar_ActionRequested;
            }

            _bar.SetBusy(false);
            _bar.SetActions(actions);
        }

        private void PositionBar()
        {
            if (_bar == null) return;

            var layer = _view.GetAdornmentLayer(LayerName);
            if (layer == null) return;
            layer.RemoveAllAdornments();

            SnapshotSpan span;
            try
            {
                span = _currentSpan.TranslateTo(_view.TextSnapshot, SpanTrackingMode.EdgeInclusive);
            }
            catch
            {
                Hide();
                return;
            }

            var end = span.End;
            if (end.Position > _view.TextSnapshot.Length)
            {
                Hide();
                return;
            }

            var line = _view.TextViewLines.GetTextViewLineContainingBufferPosition(end);
            if (line == null)
            {
                Hide();
                return;
            }

            var bounds = line.GetCharacterBounds(end);
            double x = bounds.Left - _view.ViewportLeft;
            double y = bounds.Bottom - _view.ViewportTop + 6;

            _bar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var width = Math.Max(80, _bar.DesiredSize.Width);
            var height = Math.Max(28, _bar.DesiredSize.Height);

            if (x + width > _view.ViewportWidth - 8)
                x = Math.Max(8, _view.ViewportWidth - width - 8);
            if (x < 8) x = 8;
            if (y + height > _view.ViewportHeight - 8)
                y = Math.Max(8, bounds.Top - _view.ViewportTop - height - 6);

            Canvas.SetLeft(_bar, x);
            Canvas.SetTop(_bar, y);
            layer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, "QuickActionBar", _bar, null);
        }

        private void Hide()
        {
            if (_busy) return;
            try
            {
                _view.GetAdornmentLayer(LayerName)?.RemoveAllAdornments();
            }
            catch
            {
                // view 可能已关闭
            }
        }

        private void Bar_ActionRequested(object sender, QuickActionItem action)
        {
            if (_busy || action == null) return;
            var span = _currentSpan;
            _busy = true;
            _bar?.SetBusy(true, "AI 处理中：「" + action.Title + "」");

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await QuickActionRunner.RunAsync(_view, span, action);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"QuickAction run failed: {ex.Message}");
                    try { await VS.StatusBar.ShowMessageAsync("AI 指令失败：" + ex.Message); }
                    catch { /* ignore */ }
                }
                finally
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _busy = false;
                    Refresh();
                }
            });
        }
    }
}
