using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnTranslate.NetworkTranslates.Interfaces
{
    public abstract class ANetworkTranslate
    {
        public abstract Task<List<string>> Translate(List<string> texts, string fromLan, string toLan);


        public string ArrayToString(List<string> @this)
        {
            return string.Join("   #######  ", @this);
        }

        public List<string> StringToArray(string @this) 
        {
            return @this.Split(new String[] { "#######" }, StringSplitOptions.None).ToList();
        }
    }
}
