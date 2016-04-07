using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace Bender
{
    public class Mapper : IMapper
    {
        public MapperConfig Config { get; private set; }
        public MappingContext Context { get; private set; }

        public Mapper()
        {
            Config = new MapperConfig();
        }

        public T Map<S, T>(S source)
        {
            object t = Map(source, typeof(S), null, typeof(T));
            return t != null ? (T) t : default(T);
        }

        public T Map<T>(object source, Type sourceType)
        {
            object t = Map(source, sourceType, null, typeof(T));
            return t != null ? (T) t : default(T);
        }

        public void Map<S, T>(S source, T target)
        {
            Map(source, typeof(S), target, typeof(T));
        }

        public void Map<T>(object source, Type sourceType, T target)
        {
            Map(source, sourceType, target, typeof(T));
        }

        public object Map(object source, Type sourceType, Type targetType)
        {
            return Map(source, sourceType, null, targetType);
        }

        public object Map(object source, Type sourceType, object target, Type targetType)
        {
            var sourceMappingProvider = GetMappingItemProvider(sourceType);
            var sourceMappingItems = sourceMappingProvider.GetMappingItems(source, sourceType, MappingProviderMode.Source).ToList();

            var targetMappingProvider = GetMappingItemProvider(targetType);
            var targetMappingItems = targetMappingProvider.GetMappingItems(target, targetType, MappingProviderMode.Target).ToList();
            Context = new MappingContext(sourceMappingProvider, targetMappingProvider, sourceMappingItems, targetMappingItems);

            InitMappingItems();

            PrintMappingItems();

            var rootItems = FindRootMappingItems(Context.TargetMappingItems);
            if(rootItems != null && rootItems.Count() > 1)
            {
                return rootItems.Select(ri => ri.Value);
            }
            return rootItems.Select(ri => ri.Value).SingleOrDefault();
        }

        private void PrintMappingItems()
        {
            Debug.Write(Environment.NewLine);
            PrintMappingItemProvider(Context.SourceMappingProvider, MappingProviderMode.Source);
            PrintMappingItems(FindRootMappingItems(Context.SourceMappingItems));
            PrintMappingItemProvider(Context.TargetMappingProvider, MappingProviderMode.Target);
            PrintMappingItems(FindRootMappingItems(Context.TargetMappingItems));
        }

        private IList<MappingItem> FindRootMappingItems(IEnumerable<MappingItem> items)
        {
            return items.Where(mi => mi.Parent == null).ToList();
        }

        private void InitMappingItems()
        {
            var rootTargetMappingItems = FindRootMappingItems(Context.TargetMappingItems);
            InitMappingItems(rootTargetMappingItems);
        }


        public static void PrintMappingItemProvider(IMappingItemProvider provider, MappingProviderMode mode)
        {
            Debug.WriteLine(string.Format("Provider type: {0}, Provider mode: {1}", provider.GetType().Name, mode));
        }

        public static void PrintMappingItems(MappingItem item)
        {
            Debug.WriteLine(string.Format("Key: {0}, Item type: {1}, Value: {2}, Type: {3}, Source: {4}",
                    item.Key ?? "null", item.GetType().Name, 
                    item.Value ?? "null", (object) item.Type ?? "null", item.Source ?? "null"));
        }

        public static void PrintMappingItems(IEnumerable<MappingItem> items)
        {
            foreach (var i in items)
            {
                PrintMappingItems(i);
                Debug.Indent();
                PrintMappingItems(i.Children);
                Debug.Unindent();
            }
        }

        private void InitMappingItems(IEnumerable<MappingItem> targetItems)
        {
            foreach (var targetItem in targetItems)
            {
                InitMappingItem(targetItem);
            }
        }

        private void TryAssignMappingItemValue(MappingItem targetItem, object targetValue)
        {
            if(targetValue != null && targetItem.Type.IsAssignableFrom(targetValue.GetType()))
            {
                targetItem.Value = targetValue;
            }
        }

        private void InitMappingItem(MappingItem targetItem)
        {
            var sourceItem = FindMappingItem(Context.SourceMappingItems, targetItem);
            object targetValue = null;
            if(sourceItem != null)
            {
                targetValue = sourceItem.Value;
                var typeConverter = Config.FindTypeConverter(sourceItem.Type, targetItem.Type);
                if(typeConverter != null && targetValue != null)
                {
                    try
                    {
                        targetValue = typeConverter.Convert(sourceItem.Value, targetItem.Type);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format(
                            "Error converting value {0} of type {1} to type {2}. Source item key: {3}. Target item key: {4}. Converter: {5}.",
                            targetValue, sourceItem.Type, targetItem.Type, sourceItem.Key, targetItem.Key, typeConverter.GetType().Name));
                        Debug.WriteLine(ex.ToString());
                        Context.MappingErrors.Add(new MappingError(sourceItem, targetItem));
                    }
                }
                TryAssignMappingItemValue(targetItem, targetValue);
            }

            Context.CurrentSourceMappingItem = sourceItem;
            Context.CurrentTargetValue = targetValue;

            if(targetItem is ValueMappingItem)
            {
                InitValueMappingItem((ValueMappingItem) targetItem);
            }
            if(targetItem is EnumerableMappingItem)
            {
                InitEnumerableMappingItem((EnumerableMappingItem) targetItem);
            }
            if(targetItem is ContainerMappingItem)
            {
                InitContainerMappingItem((ContainerMappingItem) targetItem);
            }

            InitMappingItems(targetItem.Children);
        }

        private void InitValueMappingItem(ValueMappingItem item)
        {
            if(Context.CurrentSourceMappingItem == null) { return; }
            item.Provider.InitMappingItem(Context, item);
        }

        private void InitContainerMappingItem(ContainerMappingItem item)
        {
            var sourceItem = Context.CurrentSourceMappingItem;
            var sourceDescendantItems = FindDescendantMappingItems(Context.SourceMappingItems, item);
            if(sourceItem == null && sourceDescendantItems.Count == 0) { return; }

            item.Provider.InitMappingItem(Context, item);
        }

        private void InitEnumerableMappingItem(EnumerableMappingItem item)
        {
            var sourceItem = Context.CurrentSourceMappingItem as EnumerableMappingItem;
            var sourceChildItems = FindChildMappingItems(Context.SourceMappingItems, item);
            if(sourceItem == null && sourceChildItems.Count == 0) { return; }

            item.Provider.InitMappingItem(Context, item);
        }

        public bool MatchMappingItemType(MappingItem sourceItem, MappingItem targetItem)
        {
            EnumerableMappingItem enumSourceItem = sourceItem as EnumerableMappingItem;
            EnumerableMappingItem enumTargetItem = targetItem as EnumerableMappingItem;
            if(enumSourceItem != null && enumTargetItem != null)
            {
                return enumTargetItem.ChildrenType == enumSourceItem.ChildrenType;
            }
            
            return targetItem.GetType() == sourceItem.GetType();
        }

        public MappingItem FindMappingItem(IEnumerable<MappingItem> items, MappingItem targetItem)
        {
            items = items.Where(i => MatchMappingItemKey(i.Key, targetItem.Key));

            var matchingItemSameMappingType = FindSingleMappingItemOrThrow(
                items.Where(i => MatchMappingItemType(i, targetItem)), targetItem);
            if(matchingItemSameMappingType != null) return matchingItemSameMappingType;
            
            var matchingItem = FindSingleMappingItemOrThrow(items, targetItem);
            return matchingItem;
        }

        private MappingItem FindSingleMappingItemOrThrow(IEnumerable<MappingItem> items, MappingItem targetItem)
        {
            if(items.Count() > 1)
            {
                var matchingItemsMessage = "";
                int count = 1;
                foreach (var il in items)
                {
                    matchingItemsMessage += string.Format("\n\tItem {0}, key {1}, type {2}", count++, il.Key, il.Type);
                }

                throw new InvalidOperationException(
                    string.Format("Too much mapping items match the item having key {0} and type {1}. ",
                        targetItem.Key, targetItem.Type) +
                    "Matching mapping items are:" + matchingItemsMessage);
            }

            return items.SingleOrDefault();
        }

        public IList<MappingItem> FindChildMappingItems(IEnumerable<MappingItem> items, MappingItem targetItem)
        {
            return items.Where(i => targetItem.Children.Any(
                ti => MatchMappingItemKey(i.Key, ti.Key))).ToList();
        }

        public IList<MappingItem> FindDescendantMappingItems(IEnumerable<MappingItem> items, MappingItem targetItem)
        {
            return FindChildMappingItems(items, targetItem).Concat(
                targetItem.Children.SelectMany(ti => FindChildMappingItems(items, ti))).ToList();
        }

        private bool MatchMappingItemKey(string sourceKey, string targetKey)
        {
            if(sourceKey == targetKey) { return true; }

            string filteredSourceKey = Config.DefaultKeyFilter.Filter(sourceKey);
            string filteredTargetKey = Config.DefaultKeyFilter.Filter(targetKey);

            bool match = false;
            if(filteredSourceKey != sourceKey)
            {
                match = MatchMappingItemKey(filteredSourceKey, targetKey) ||
                    MatchMappingItemKey(filteredSourceKey, filteredTargetKey);
            }
            else if(filteredTargetKey != targetKey)
            {
                match = MatchMappingItemKey(sourceKey, filteredTargetKey);
            }
            return match;
        }

        private IMappingItemProvider GetMappingItemProvider(Type itemType)
        {
            var mappingItemProvider = Config.FindMappingItemProvider(itemType);
            return mappingItemProvider;
        }

    }
}

