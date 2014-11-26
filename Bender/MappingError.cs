using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{
    public class MappingError
    {
        public static readonly IDictionary<Type, string> DefaultErrorMessages = new Dictionary<Type, string>() {
            { typeof(DateTime), "Il valore {0} non è valido per il campo {1} di tipo data." },
            { typeof(int), "Il valore {0} non è valido per il campo {1} di tipo intero." },
            { typeof(float), "Il valore {0} non è valido per il campo {1} di tipo decimale." },
            { typeof(double), "Il valore {0} non è valido per il campo {1} di tipo decimale." },  
            { typeof(decimal), "Il valore {0} non è valido per il campo {1} di tipo decimale." }  
        };
        
        MappingItem SourceItem { get; set; }
        MappingItem TargetItem { get; set; }

        public MappingError(MappingItem sourceItem, MappingItem targetItem)
        {
            SourceItem = sourceItem;
            TargetItem = targetItem;
        }

        public override string ToString()
        {
            var msg = DefaultErrorMessages[TargetItem.Type];
            if(msg != null)
            {
                return string.Format(msg, SourceItem.Value, TargetItem.Name);
            }
            return null;
        }
    }
}
