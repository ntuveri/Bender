using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{
    public class MapperConfig
    {
        public IList<ITypeConverter> TypeConverters { get; private set; }
        public IList<IMappingItemProvider> MappingItemProviders { get; private set; }
        public IKeyFilter DefaultKeyFilter { get; set; }

        public MapperConfig()
        {
            TypeConverters = new List<ITypeConverter>();
            TypeConverters.Add(new NativeTypeConverter());
            
            MappingItemProviders = new List<IMappingItemProvider>();
            MappingItemProviders.Add(new WebControlsMappingItemProvider());
            MappingItemProviders.Add(new ObjectMappingItemProvider());

            DefaultKeyFilter = new CompositeFilter(
                new IKeyFilter[] {
                    new IdentityFilter(), 
                    new PrefixPostfixFilter(
                        new string[] { "lbl", "txt", "hdn", "grd", "pnl", "rpt", "rbl", "cbl", "drp", "ddl", "cb", "rb" }, 
                        new string[] { "Selected" } ),
                    new DotFilter()
                });
        }

        public ITypeConverter FindTypeConverter(Type sourceType, Type targetType)
        {
            for (int i = 0; i < TypeConverters.Count; i++)
            {
                ITypeConverter tc = TypeConverters[i];
                if (tc.CanConvert(sourceType, targetType)) { return tc; }
            }
            return null;
        }

        public IMappingItemProvider FindMappingItemProvider(Type rootType)
        {
            for (int i = 0; i < MappingItemProviders.Count; i++)
            {
                var mip = MappingItemProviders[i];
                if(mip.MappedRootType.IsAssignableFrom(rootType)) { return mip; }
            }
            return null;
        }
    }
}
