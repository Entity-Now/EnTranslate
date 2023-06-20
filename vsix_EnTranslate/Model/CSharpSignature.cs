using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vsix_EnTranslate.Model
{
    public class CSharpSignature
    {
        public string Documentation { get; }

        public CSharpSignature(string documentation)
        {
            Documentation = documentation;
        }
    }
}
