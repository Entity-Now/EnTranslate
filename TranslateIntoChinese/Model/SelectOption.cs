using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TranslateIntoChinese.Model.Enums;

namespace TranslateIntoChinese.Model
{
    public class SelectOption<T> where T : Enum
    {
        public string Name { get; set; }
        public T Value { get; set; }
        public Type? Instance { get; set; }
    }
}
