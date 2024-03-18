using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace TranslateIntoChinese.Core
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("ToolTip QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType("csharp"), ContentType("C/C++"), ContentType("css"), ContentType("F#"), ContentType("JScript"), ContentType("JS"), ContentType("JavaScript"), ContentType("Json"), ContentType("txt"), ContentType("html"), ContentType("cshtml")]
    internal class EnQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new EnQuickInfoSource(this, textBuffer);
        }
    }
}
