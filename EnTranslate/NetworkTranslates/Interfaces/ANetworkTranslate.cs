using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnTranslate.NetworkTranslates.Interfaces
{
    public abstract class ANetworkTranslate
    {
        public abstract Task<string> GetToken(); 
        public abstract Task<List<string>> Translate(List<string> texts, string fromLan, string toLan, string token = null);
        public abstract Task<string> Detect(string text, string token = null);


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
