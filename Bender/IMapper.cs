using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{
    public interface IMapper
    {

        T Map<S, T>(S source);

        T Map<T>(object source, Type sourceType);

        void Map<S, T>(S source, T target);

        void Map<T>(object source, Type sourceType, T target);

        object Map(object source, Type sourceType, Type targetType);

        object Map(object source, Type sourceType, object target, Type targetType);

    }
}
