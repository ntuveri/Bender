using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{
    public interface IMappingItemProvider
    {
        Type MappedRootType { get; }
        
        IEnumerable<MappingItem> GetMappingItems(object item, Type itemType, MappingProviderMode mode);
        IEnumerable<MappingItem> GetMappingItems(object item, Type itemType, MappingItem parentMappingItem, MappingProviderMode mode);
        
        void InitMappingItem(MappingContext context, ValueMappingItem valueItem);
        void InitMappingItem(MappingContext context, ContainerMappingItem containerItem);
        void InitMappingItem(MappingContext context, EnumerableMappingItem enumItem);
    }
}
