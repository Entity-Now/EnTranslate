using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace TranslateIntoChinese.Core
{
    [ContentType("text")]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class TextViewCreationListener : IWpfTextViewCreationListener
    {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("TranslatorAdornmentLayer")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        [Order(After = PredefinedAdornmentLayers.Outlining)]
        [Order(After = PredefinedAdornmentLayers.Selection)]
        [Order(After = PredefinedAdornmentLayers.Squiggle)]
        [Order(After = PredefinedAdornmentLayers.Text)]
        [Order(After = PredefinedAdornmentLayers.TextMarker)]
        public AdornmentLayerDefinition TranslatorLayerDefinition;

        private IWpfTextView wpfTextView;
        public void TextViewCreated(IWpfTextView textView)
        {
            wpfTextView = textView;
            // 暂时不启用
            //wpfTextView.Selection.SelectionChanged += Selection_SelectionChange;
            //wpfTextView.Selection.
        }
        public void Selection_SelectionChange(object sender, EventArgs e)
        {
            ITextSelection selection = sender as ITextSelection;
            if (selection != null)
            {
                var span = selection.SelectedSpans[0];
                string selectedText = span.GetText();
                var hasSelectedText = !string.IsNullOrWhiteSpace(selectedText);
                if (hasSelectedText)
                {
                    // 此处可以做一些操作
                }
            }
        }
        [Import]
        internal IEditorFormatMapService FormatMapService = null;
    }
}
