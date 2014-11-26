using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{
    public interface IMappingItemProvider
    {
        Type MappedRootType { get; }
        MappingProviderMode MappingProviderMode { get; set; }
        
        IEnumerable<MappingItem> GetMappingItems(object item, Type itemType);
        IEnumerable<MappingItem> GetMappingItems(object item, Type itemType, MappingItem parentMappingItem);
        
        void InitMappingItem(MappingContext context, ValueMappingItem valueItem);
        void InitMappingItem(MappingContext context, ContainerMappingItem containerItem);
        void InitMappingItem(MappingContext context, EnumerableMappingItem enumItem);
    }
}
