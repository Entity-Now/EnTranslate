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
    [ContentType("text"), ContentType("code"), ContentType("projection")]
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
