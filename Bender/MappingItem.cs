using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bender
{

    public abstract class MappingItem
    {
        public string Key 
        { 
            get
            {
                if(Parent != null) 
                {
                    if(Parent is EnumerableMappingItem)
                    {
                        return Parent.Key + "[" + Index + "]"; 
                    }
                    else if(Parent is ContainerMappingItem) 
                    {
                        return (Parent.Key != null ? Parent.Key + "." : "") + Name;
                    }
                }
                return null;
            }
        }
        public string Name { get; internal set; }
        public int? Index 
        { 
            get
            {
                if(Parent is EnumerableMappingItem) 
                {
                    return Parent.Children.IndexOf(this);
                }
                return null;
            }
        }

        public IMappingItemProvider Provider { get; set; }

        public int Level 
        { 
            get
            {
                var ancestor = Parent;
                var i = 0;
                while(ancestor != null) 
                {
                    ancestor = ancestor.Parent;
                    i++;
                }
                return i;
            }
        }

        private MappingItem parent;
        public MappingItem Parent
        {
            get { return parent; }
            set 
            {
                if(parent != value)
                {
                    if(parent != null)
                    {
                        parent.Children.Remove(this);   
                    }

                    parent = value;
                    
                    if(parent != null)
                    {
                        parent.Children.Add(this);   
                    }
                }
            }
        }

        private List<MappingItem> children;
        public IList<MappingItem> Children 
        { 
            get 
            {
                if(children == null) { children = new List<MappingItem>(); }
                return children;
            }
        }

        public Type Type { get; set; }
        public object Value { get; set; }
        internal object Source { get; set; }
    }

    public class ValueMappingItem : MappingItem
    {
    }

    public class ContainerMappingItem : MappingItem
    {
    }

    public class EnumerableMappingItem : MappingItem
    {
        public int ElementsCount 
        { 
            get { return Children.Count; } 
        } 
        public Type ElementType { get; set; } 
    }   
}
