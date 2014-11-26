using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace Bender
{
    public class ObjectMappingItemProvider : IMappingItemProvider
    {
        public MappingProviderMode MappingProviderMode { get; set; }

        public Type MappedRootType
        {
            get { return typeof(object); }
        }

        private bool IsValueType(Type itemType)
        {
            return itemType.IsPrimitive || itemType == typeof(string) || 
                itemType == typeof(DateTime) || itemType == typeof(Nullable<DateTime>) ||
                (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                itemType.GetGenericArguments()[0].IsPrimitive) || 
                itemType.IsEnum;
        }

        private bool IsEnumerableType(Type itemType)
        {
            var itemTypeInterfaces = from iti in itemType.GetInterfaces()
                                     where iti.IsGenericType
                                     select iti.GetGenericTypeDefinition();
            if(itemType.IsArray || (itemType.IsGenericType && itemTypeInterfaces.Any(iti => iti == typeof(IEnumerable<>))))
            {
                return true;
            }   
            if(itemType.BaseType != null && itemType.BaseType != typeof(object))
            {
                return IsEnumerableType(itemType.BaseType);
            }
            return false;
        }

        private Type GetEnumerableElementType(Type itemType)
        {
            if (itemType.IsGenericType && !itemType.IsGenericTypeDefinition)
            {
                return itemType.GetGenericArguments()[0];
            }
            if (itemType.IsArray)
            {
                return itemType.GetElementType();
            }
            return typeof(object);
        }
        
        private int GetEnumerableCount(object item)
        {
            IEnumerable enumItem = item as IEnumerable;
            if(enumItem == null) { return 0; }
            int i = 0;
            foreach (var ei in enumItem)
            {
                i++;
            }
            return i;
        }

        private bool IsDictionaryType(Type itemType)
        {
            var itemTypeInterfaces = from iti in itemType.GetInterfaces()
                                     where iti.IsGenericType
                                     select new {
                                         TypeDefinition = iti.GetGenericTypeDefinition(),
                                         TypeArguments = iti.GetGenericArguments()
                                     };
            if(itemType.IsGenericType && itemTypeInterfaces.Any(iti => 
                    iti.TypeDefinition == typeof(IDictionary<,>) && iti.TypeArguments.Length > 0 && 
                    iti.TypeArguments[0] == typeof(string)))
            {
                return true;
            }   
            if(itemType.BaseType != null && itemType.BaseType != typeof(object))
            {
                return IsDictionaryType(itemType.BaseType);
            }
            return false;
        }

        private Type GetDictionaryValueType(Type itemType)
        {
            if (itemType.IsGenericType && !itemType.IsGenericTypeDefinition)
            {
                return itemType.GetGenericArguments()[1];
            }
            return typeof(object);
        }

        
        public void InitMappingItem(MappingContext context, ValueMappingItem valueItem)
        {
            var setter = GetSetter(valueItem);
            if(setter != null) 
            {
                setter(valueItem.Value); // TODO: handle not assignable values
            }
        }

        public void InitMappingItem(MappingContext context, ContainerMappingItem containerItem)
        {
            if(containerItem.Value == null)
            {
                object containerValue = Activator.CreateInstance(containerItem.Type, new object[] {});
                if(IsDictionaryType(containerItem.Type))
                {
                    var dict = containerValue as IDictionary;
                    foreach (var key in context.CurrentSourceMappingItem.Children.Select(si => si.Name))
                    {
                        var valueType = GetDictionaryValueType(containerItem.Type);
                        dict[key] = valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
                        var subMappingItems = GetMappingItems(dict[key], valueType, containerItem);
                        foreach (var smi in subMappingItems)
                        {  
                            if(smi.Parent == containerItem) { smi.Name = (string) key; }
                            context.TargetMappingItems.Add(smi);
                        }
                    }
                }
                containerItem.Value = containerValue;
            }
            var setter = GetSetter(containerItem);
            if(setter != null)
            {
                setter(containerItem.Value);
            }
        }

        public void InitMappingItem(MappingContext context, EnumerableMappingItem enumItem)
        {
            if(enumItem.Value == null || enumItem.ElementsCount == 0)
            {
                object enumValue = null;
                int enumCount = context.CurrentSourceMappingItem.Children.Count;
                // array creation
                object ev = Array.CreateInstance(enumItem.ElementType, enumCount);
                // collection creation 
                enumValue = enumItem.Type.IsArray ? ev : Activator.CreateInstance(enumItem.Type, new object[] { ev });
                enumItem.Value = enumValue;
                
                foreach (var evItem in (IEnumerable) enumValue)
                {
                    var targetItems = GetMappingItems(evItem, enumItem.ElementType, enumItem);
                    foreach(var ti in targetItems) 
                    {
                        context.TargetMappingItems.Add(ti); 
                    }
                }
            }
            var setter = GetSetter(enumItem);
            if(setter != null)
            {
                setter(enumItem.Value);
            }
        }

        public IEnumerable<MappingItem> GetMappingItems(object item, Type itemType, MappingItem parentMappingItem)
        {
            if (IsValueType(itemType))
            {
                var valueMappingItem = new ValueMappingItem() {
                    Parent = parentMappingItem,
                    Value = item,
                    Type = itemType,
                    Provider = this
                };
                yield return valueMappingItem;
            }
            else if(IsEnumerableType(itemType) && !IsDictionaryType(itemType))
            {   
                var enumMappingItem = new EnumerableMappingItem() {
                    Parent = parentMappingItem,
                    Value = item,
                    Type = itemType, 
                    ElementType = GetEnumerableElementType(itemType),
                    Provider = this
                };
                yield return enumMappingItem;

                if(item != null)
                {
                    foreach(var element in (IEnumerable) item)
                    {
                        var subMappingItems = GetMappingItems(element, enumMappingItem.ElementType, enumMappingItem);
                        foreach (var smi in subMappingItems)
                        {
                            yield return smi;
                        }
                    }
                }
            }
            else if(IsDictionaryType(itemType))
            {   
                ContainerMappingItem containerMappingItem = new ContainerMappingItem() {
                    Parent = parentMappingItem,
                    Value = item,
                    Type = itemType, 
                    Provider = this
                };
                yield return containerMappingItem;

                var dictionary = item as IDictionary;
                if(dictionary != null) 
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        var valueType = value != null ? value.GetType() : GetDictionaryValueType(itemType);
                        var subMappingItems = GetMappingItems(value, valueType, containerMappingItem);
                    
                        foreach (var smi in subMappingItems)
                        {  
                            if(smi.Parent == containerMappingItem) { smi.Name = (string) key; }
                            yield return smi;
                        }
                    }
                }
            }
            else
            {
                ContainerMappingItem containerMappingItem = new ContainerMappingItem() {
                    Parent = parentMappingItem,
                    Value = item,
                    Type = itemType, 
                    Provider = this
                };
                yield return containerMappingItem;

                var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var propValue = item == null ? null : prop.GetValue(item, null); 
                    var subMappingItems = GetMappingItems(propValue, prop.PropertyType, containerMappingItem);
                    
                    foreach (var smi in subMappingItems)
                    {  
                        if(smi.Parent == containerMappingItem) { smi.Name = prop.Name; }
                        yield return smi;
                    }
                }
            }
        }


        public IEnumerable<MappingItem> GetMappingItems(object root, Type rootType)
        {
            return GetMappingItems(root, rootType, null);
        }

        private Action<object> GetSetter(MappingItem mappingItem)
        {
            if(mappingItem.Parent != null) 
            {
                if(mappingItem.Parent is EnumerableMappingItem)
                {
                    return GetEnumerableSetter(mappingItem); 
                }
                else if(mappingItem.Parent is ContainerMappingItem) 
                {
                    return GetContainerSetter(mappingItem);
                }
            }
            return null;
        }

        private Action<object> GetContainerSetter(MappingItem item)
        {
            ContainerMappingItem parentItem = (ContainerMappingItem) item.Parent;
            if(IsDictionaryType(parentItem.Type))
            {     
                var dictionary = parentItem.Value as IDictionary;
                return (val) => {
                    if(dictionary != null) 
                    { 
                        dictionary[item.Name] = val;
                    }
                };
            }
            PropertyInfo propInfo = parentItem.Type.GetProperty(item.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); 
            return (val) => {
                if(parentItem.Value != null && propInfo != null) 
                { 
                    propInfo.SetValue(parentItem.Value, val, null); 
                }
            };
        }

        private Action<object> GetEnumerableSetter(MappingItem item)
        {
            EnumerableMappingItem parentItem = (EnumerableMappingItem) item.Parent;
            MethodInfo methodInfo = 
                parentItem.Type.GetMethod("SetValue", new Type[] { parentItem.ElementType, typeof(int) }) ?? // array
                parentItem.Type.GetMethod("set_Item", new Type[] { typeof(int), parentItem.ElementType }); // list
            
            return (val) => {
                if(parentItem.Value != null && methodInfo != null) 
                { 
                    object[] methodParams = null;
                    if(methodInfo != null && methodInfo.GetParameters()[0].ParameterType == typeof(int))
                    {
                        methodParams = new object[] { item.Index.Value, val };
                    }
                    else
                    {
                        methodParams = new object[] { val, item.Index.Value };
                    }

                    methodInfo.Invoke(parentItem.Value, methodParams); 
                }
            };
        }
    }
}
