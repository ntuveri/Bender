﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Diagnostics;
using System.Collections;

namespace Bender
{
    public class WebControlsMappingItemProvider : IMappingItemProvider
    {
        public MappingProviderMode MappingProviderMode { get; set; }

        public Type MappedRootType
        {
            get { return typeof(Control); }
        }


        public interface IWebControlAdapter 
        {
            IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode);
        }

        public class ValueWebControlAdapter : IWebControlAdapter
        {
            public Type ControlType { get; set; }
            public Type ValueType { get; set; }
            public Func<Control, object> Getter { get; set; }
            public Action<Control, object> Setter { get; set; }

            public virtual IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode)
            {
                if(!(control is CheckBox && parentMappingItem.Source is CheckBoxList))
                {
                    yield return new ValueMappingItem() {
                            Parent = parentMappingItem,
                            Type = ValueType,
                            Value = Getter(control),
                            Provider = provider,
                            Name = control.ID,
                            Source = control
                    };
                }
            }
        }
        
        public class EnumerableWebControlAdapter : IWebControlAdapter
        {
            public Type ControlType { get; set; }
            public Func<Control, object> Getter { get; set; }
            public Action<Control, object, MappingContext> Setter { get; set; }

            public virtual IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode)
            {
                 yield return new EnumerableMappingItem() {
                    Parent = parentMappingItem,
                    Value = Getter(control),
                    Type = typeof(IEnumerable),
                    ElementType = typeof(object),
                    Provider = provider,
                    Name = control.ID,
                    Source = control
                };
            }
        }

        public class ContainerWebControlAdapter : IWebControlAdapter
        {
            public Type ControlType { get; set; }
            public virtual IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode)
            {
                yield return new ContainerMappingItem() {
                        Parent = parentMappingItem,
                        Value = null,
                        Type = typeof(object),
                        Provider = provider,
                        Source = control,
                        Name = control.ID    
                    };
            }
        }

        public class ListControlWebControlAdapter : EnumerableWebControlAdapter
        {
            public ListControlWebControlAdapter()
            {
                ControlType = typeof(ListControl);
                Getter = ctrl => {
                    var lc = (ListControl) ctrl;
                    return lc.DataSource ?? (lc.Items.Count > 0 ? new object[lc.Items.Count] : null);
                };
                Setter = (ctrl, val, ctx) => {
                    ListControl listControl = (ListControl) ctrl;
                    if(listControl.DataSource == null && listControl.Items.Count == 0)
                    {
                        listControl.DataSource = val;
                        listControl.DataBind();
                    }                    
                    var selectedMappingItems = ctx.SourceMappingItems.
                        Where(smi => 
                            smi.Key == ctx.CurrentSourceMappingItem.Key ||
                            smi.Key == ctx.CurrentSourceMappingItem.Key + "Selected");
                    var selectedValues = selectedMappingItems.OfType<ValueMappingItem>().Concat(selectedMappingItems.
                        OfType<EnumerableMappingItem>().SelectMany(smi => smi.Children).OfType<ValueMappingItem>()).
                        Select(smi => smi.Value.ToString());

                    foreach (var sv in selectedValues)
                    {
                        ListItem li = listControl.Items.FindByValue(sv);
                        if(li != null) { li.Selected = true; }                                    
                    }
                };
            }

            
            private IEnumerable<MappingItem> InternalGetMappingItems(Control control, 
                    MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode, 
                    bool isSelected, bool isMultiSelect)
            {
                MappingItem enumItem = null; 
                ListControl lc = (ListControl) control;
                var listItems = lc.Items.Cast<ListItem>().Where(li => !isSelected || (isSelected && li.Selected) ||
                    mode == MappingProviderMode.Target);
                
                if(!isSelected || isMultiSelect)
                {
                    enumItem = new EnumerableMappingItem() {
                        Parent = parentMappingItem,
                        Value = !isSelected ? Getter(control) : new object[listItems.Count()],
                        Type = typeof(IEnumerable),
                        ElementType = isSelected ? typeof(string) : typeof(object),
                        ChildrenType = isSelected ? typeof(ValueMappingItem) : typeof(ContainerMappingItem),
                        Provider = provider,
                        Name = control.ID + (isSelected ? "Selected" : null),
                        Source = !isSelected ? control : null,
                    };
                    yield return enumItem;
                }

                foreach (ListItem li in listItems)
                {
                    if(isSelected) 
                    {
                        yield return new ValueMappingItem() { 
                            Parent = isMultiSelect ? enumItem : parentMappingItem,
                            Type = typeof(string),
                            Value = li.Value,
                            Provider = provider,
                            Source = new ListItemControlSelected(li),
                            Name = !isMultiSelect ? control.ID + "Selected" : null
                        };
                    }
                    else 
                    {
                        var containerItem = new ContainerMappingItem() { 
                            Parent = enumItem,
                            Type = typeof(object),
                            Provider = provider
                        };
                        yield return containerItem;
                    
                        yield return new ValueMappingItem() { 
                            Parent = containerItem,
                            Type = typeof(string),
                            Value = li.Text,
                            Provider = provider, 
                            Name = !string.IsNullOrEmpty(lc.DataTextField) ? lc.DataTextField : "Text"
                        };
                    
                        yield return new ValueMappingItem() { 
                            Parent = containerItem,
                            Type = typeof(string),
                            Value = li.Value,
                            Provider = provider, 
                            Name = !string.IsNullOrEmpty(lc.DataValueField) ? lc.DataValueField : "Value"
                        };
                    }
                }
            }

            public override IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode)
            {
                ListBox listBox = control as ListBox;
                bool isMultiSelect = (listBox != null && listBox.SelectionMode == ListSelectionMode.Multiple) || control is CheckBoxList;

                return InternalGetMappingItems(control, parentMappingItem, provider, mode, false, isMultiSelect).Concat(
                    InternalGetMappingItems(control, parentMappingItem, provider, mode, true, isMultiSelect));
            }
        }


        public class GridViewRowWebControlAdapter : ContainerWebControlAdapter
        {
            public GridViewRowWebControlAdapter()
            {
                ControlType = typeof(GridViewRow);
            }

            public override IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode)
            {
                GridViewRow gvr = (GridViewRow) control;
                if(gvr.RowType == DataControlRowType.DataRow) 
                {
                    return base.GetMappingItems(control, parentMappingItem, provider, mode);
                }
                return Enumerable.Empty<MappingItem>();
            }
        }

        
        public class RepeaterItemWebControlAdapter : ContainerWebControlAdapter
        {
            public RepeaterItemWebControlAdapter()
            {
                ControlType = typeof(RepeaterItem);
            }

            public override IEnumerable<MappingItem> GetMappingItems(Control control, MappingItem parentMappingItem, IMappingItemProvider provider, MappingProviderMode mode)
            {
                RepeaterItem ri = (RepeaterItem) control;
                if(ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem) 
                {
                    return base.GetMappingItems(control, parentMappingItem, provider, mode);
                }
                return Enumerable.Empty<MappingItem>();
            }
        }

        public class ListItemControlSelected : Control
        {
            public ListItem ListItem { get; private set; }
            public ListItemControlSelected(ListItem li)
            {
                ListItem = li;
            }
        }

        public static IList<ValueWebControlAdapter> ValueWebControls = new List<ValueWebControlAdapter>()
        {
            new ValueWebControlAdapter() { 
                ControlType = typeof(TextBox), 
                ValueType = typeof(string),
                Getter = c => ((TextBox) c).Text, 
                Setter = (c, v) => ((TextBox) c).Text = (string) v
            },
            new ValueWebControlAdapter() { 
                ControlType = typeof(Label), 
                ValueType = typeof(string),
                Getter = c => ((Label) c).Text, 
                Setter = (c, v) => ((Label) c).Text = (string) v
            },
            new ValueWebControlAdapter() { 
                ControlType = typeof(HiddenField), 
                ValueType = typeof(string),
                Getter = c => ((HiddenField) c).Value, 
                Setter = (c, v) => ((HiddenField) c).Value = (string) v
            },
            new ValueWebControlAdapter() { 
                ControlType = typeof(CheckBox), 
                ValueType = typeof(bool),
                Getter = c => ((CheckBox) c).Checked, 
                Setter = (c, v) => ((CheckBox) c).Checked = (bool) v
            },
            new ValueWebControlAdapter() { 
                ControlType = typeof(ListItemControlSelected), 
                ValueType = typeof(string),
                Getter = c => ((ListItemControlSelected) c).ListItem.Value, 
                Setter = (c, v) => {
                    var li = ((ListItemControlSelected) c).ListItem;
                    li.Selected = string.Equals(li.Value, (string) v, StringComparison.InvariantCultureIgnoreCase);
                }
            }
        };

        public static IList<EnumerableWebControlAdapter> EnumerableWebControls = new List<EnumerableWebControlAdapter>()
        {
            new EnumerableWebControlAdapter() { 
                ControlType = typeof(GridView), 
                Getter = ctrl => {
                    var gv = (GridView) ctrl;
                    return gv.DataSource ?? (gv.Rows.Count > 0 ? new object[gv.Rows.Count] : null);  
                },
                Setter = (ctrl, val, ctx) => {
                    var gv = (GridView) ctrl;
                    if(gv.DataSource == null && gv.Rows.Count == 0)
                    {
                        gv.DataSource = val;
                        gv.DataBind();
                    }
                }
            },
            new ListControlWebControlAdapter(),
            new EnumerableWebControlAdapter() { 
                ControlType = typeof(Repeater), 
                Getter = ctrl => {
                    var r = (Repeater) ctrl;
                    return r.DataSource ?? (r.Items.Count > 0 ? new object[r.Items.Count] : null);  
                },
                Setter = (ctrl, val, ctx) => { 
                    var r = (Repeater) ctrl;
                    if(r.DataSource == null && r.Items.Count == 0)
                    {
                        r.DataSource = val;
                        r.DataBind();
                    }
                }
            },
        };

        public static IList<ContainerWebControlAdapter> ContainerWebControls = new List<ContainerWebControlAdapter>()
        {
            new ContainerWebControlAdapter() { ControlType = typeof(Page) },
            new ContainerWebControlAdapter() { ControlType = typeof(UserControl) },
            new ContainerWebControlAdapter() { ControlType = typeof(Panel) },
            new GridViewRowWebControlAdapter()
        };

        public IEnumerable<MappingItem> GetMappingItems(object root, Type rootType, MappingProviderMode mode)
        {
            if(!typeof(Control).IsAssignableFrom(rootType))
            {
                return Enumerable.Empty<MappingItem>();
            }
            
            return GetMappingItems(root, rootType, null, mode);
        }

        private IWebControlAdapter GetWebControl(Control control, Type controlType) 
        {
            return (IWebControlAdapter) ValueWebControls.SingleOrDefault(vwc => vwc.ControlType.IsAssignableFrom(controlType)) ?? 
                (IWebControlAdapter) EnumerableWebControls.SingleOrDefault(ewc => ewc.ControlType.IsAssignableFrom(controlType)) ?? 
                (IWebControlAdapter) ContainerWebControls.SingleOrDefault(vwc => vwc.ControlType.IsAssignableFrom(controlType)); 
        }

        public IEnumerable<MappingItem> GetMappingItems(object item, Type itemType, MappingItem parentMappingItem, MappingProviderMode mode)
        {   
            Control control = (Control) item;
            Type controlType = itemType;

            MappingItem tmpParentMappingItem = null;
            var webControl = GetWebControl(control, controlType);
            if(webControl != null)
            {
                var mappingItems = webControl.GetMappingItems(control, parentMappingItem, this, mode);
                foreach(var mi in mappingItems)
                {
                    // parent is the first mappging item
                    if(tmpParentMappingItem == null) { tmpParentMappingItem = mi; }
                    yield return mi;
                }
            }
            foreach (Control sc in control.Controls)
            {
                var subMappingItems = GetMappingItems(sc, sc.GetType(), tmpParentMappingItem ?? parentMappingItem, mode);
                foreach(var smi in subMappingItems)
                {
                    yield return smi;
                }
            }
        }

        public void InitMappingItem(MappingContext context, ValueMappingItem valueItem)
        {
            // var value = context.CurrentTargetValue;
            var value = valueItem.Value;
            var valueControl = ValueWebControls.SingleOrDefault(vwc => 
                valueItem.Source != null && vwc.ControlType.IsAssignableFrom(valueItem.Source.GetType())); 
            if(valueControl != null)
            {
                try
                {
                    valueControl.Setter((Control)valueItem.Source, value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());  
                }
            }
        }

        public void InitMappingItem(MappingContext context, ContainerMappingItem containerItem)
        {
            // nothing to do
        }

        public void InitMappingItem(MappingContext context, EnumerableMappingItem enumItem)
        {
            // var value = context.CurrentTargetValue;
            var value = enumItem.Value;
            var enumControl = EnumerableWebControls.SingleOrDefault(ewc => 
                enumItem.Source != null && ewc.ControlType.IsAssignableFrom(enumItem.Source.GetType())); 
            if(enumControl != null) 
            {
                var control = (Control) enumItem.Source;
                var initialValue = enumControl.Getter(control);
                enumControl.Setter(control, value, context);
                
                if(initialValue == null)
                {   
                    var subMappingItems = GetMappingItems(control, control.GetType(), MappingProviderMode.Target).ToList();
                    foreach (var smi in subMappingItems)
                    {
                        if(smi.Parent == null) 
                        {
                            foreach (var smic in smi.Children.ToList())
                            {
                                smic.Parent = enumItem;
                            }
                        }  
                        else
                        {
                            context.TargetMappingItems.Add(smi);
                        }
                    }
                }
            }
        }
    }
}
