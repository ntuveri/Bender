using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{
    public class MappingContext
    {
        public MappingContext(IMappingItemProvider sourceMappingProvider, IMappingItemProvider targetMappingProvider) : 
                this(sourceMappingProvider, targetMappingProvider, new List<MappingItem>(), new List<MappingItem>())
        {
        }

        public MappingContext(IMappingItemProvider sourceMappingProvider, IMappingItemProvider targetMappingProvider, 
                IList<MappingItem> sourceMappingItems, IList<MappingItem> targetMappingItems)
        {
            SourceMappingProvider = sourceMappingProvider;
            TargetMappingProvider = targetMappingProvider;
            SourceMappingItems = sourceMappingItems;
            TargetMappingItems = targetMappingItems;
            MappingErrors = new List<MappingError>();
        }

        public IList<MappingItem> SourceMappingItems { get; private set; }
        public IList<MappingItem> TargetMappingItems { get; private set; }
        public IList<MappingError> MappingErrors { get; private set; }

        public IMappingItemProvider SourceMappingProvider { get; private set; }
        public IMappingItemProvider TargetMappingProvider { get; private set; }

        public MappingItem CurrentSourceMappingItem { get; set; }
        public object CurrentTargetValue { get; set; }
    }
}
