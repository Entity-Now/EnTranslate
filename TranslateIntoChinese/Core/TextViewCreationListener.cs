using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace TranslateIntoChinese.Core
{
    [ContentType("text")]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public sealed class TextViewCreationListener : IWpfTextViewCreationListener
    {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("TranslatorAdornmentLayer")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        public AdornmentLayerDefinition TranslatorLayerDefinition;

        [Export(typeof(AdornmentLayerDefinition))]
        [Name(SelectionQuickActionController.LayerName)]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition QuickActionLayerDefinition;

        public void TextViewCreated(IWpfTextView textView)
        {
            if (textView == null) return;
            _ = new TextViewHotkeyFilter(textView);
            _ = new SelectionQuickActionController(textView);
        }
    }
}
