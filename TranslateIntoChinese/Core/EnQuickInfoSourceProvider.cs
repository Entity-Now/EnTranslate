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
    [ContentType("csharp"), ContentType("C/C++"), ContentType("css"), ContentType("F#"), ContentType("JScript"), ContentType("JS"), ContentType("JavaScript"), ContentType("Json"), ContentType("text"), ContentType("html"), ContentType("cshtml")]
    public class EnQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        public ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        public ITextBufferFactoryService TextBufferFactoryService { get; set; }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new EnQuickInfoSource(this, textBuffer);
        }
    }
}
