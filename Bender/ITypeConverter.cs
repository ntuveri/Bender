using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bender
{

    public interface ITypeConverter
    {
        bool CanConvert(Type sourceType, Type targetType);
        object Convert(object source, Type targetType);
    }

    public class NativeTypeConverter : ITypeConverter 
    {
        public bool CanConvert(Type sourceType, Type targetType)
        {
            var typeConverterTarget = TypeDescriptor.GetConverter(targetType);
            var typeConverterSource = TypeDescriptor.GetConverter(sourceType);
            
            return (typeConverterTarget != null && typeConverterTarget.CanConvertFrom(sourceType)) || 
                (typeConverterSource != null && typeConverterSource.CanConvertTo(targetType));
        }

        public object Convert(object source, Type targetType)
        {
            var typeConverterTarget = TypeDescriptor.GetConverter(targetType);
            if(typeConverterTarget != null && typeConverterTarget.CanConvertFrom(source.GetType()))
            {
                return typeConverterTarget.ConvertFrom(source);
            }

            var typeConverterSource = TypeDescriptor.GetConverter(source.GetType());
            if(typeConverterSource != null && typeConverterSource.CanConvertTo(targetType))
            {
                return typeConverterSource.ConvertTo(source, targetType);
            }

            throw new InvalidOperationException(string.Format("Can't convert value {0} from type {1} to type {2}",
                source, source.GetType(), targetType));
        }
    }
}
