using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

namespace Bender.Test
{
    [TestFixture]
    public class MapperTest
    {
        class Template1 : ITemplate
        { 
            public void InstantiateIn(Control container)
            {
                Label l1 = new Label() { ID = "InnerLabel" };
                HiddenField hf1 = new HiddenField() { ID = "InnerHiddenField" };
                container.Controls.Add(l1);
                container.Controls.Add(hf1);
            }
        }
        
        class Template2 : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                TextBox tb1 = new TextBox() { ID = "InnerTextBox" };
                container.Controls.Add(tb1);
            }
        }

        class Template3 : ITemplate
        {
            public void InstantiateIn(Control container)
            {
                DropDownList dd1 = new DropDownList() { ID = "InnerDropDown", DataTextField = "Text", DataValueField = "Value" };
                container.Controls.Add(dd1);
            }
        }


        class Page1 : Page
        {
            public TextBox TextBox { get; set; }
            private HiddenField HiddenField { get; set; }
            public HiddenField NullableHiddenField { get; set; }
            public HiddenField EnumHiddenField { get; set; }
            protected Label Label { get; set; }
            public Label Label2 { get; set; }
            public GridView GridView { get; set; }
            public ListBox ListBox { get; set; }
            public RadioButton RadioButton { get; set; }
            
            public Panel Panel { get; set; }
            public Label Label3 { get; set; }
            public Label Label4 { get; set; }

            public Repeater Repeater { get; set; }
            public CheckBoxList CheckBoxList { get; set; }
            public RadioButtonList RadioButtonList { get; set; }
            public ListBox ListBox2 { get; set; }
            
            public Page1()
            {

                // prefix
                TextBox = new TextBox() { ID = "txtTextBox" };
                HiddenField = new HiddenField() { ID = "hdnHiddenField" };
                
                Label = new Label() { ID = "Label" };
                NullableHiddenField = new HiddenField() { ID = "NullableHiddenField" };
                EnumHiddenField = new HiddenField() { ID = "EnumHiddenField" };
                Label2 = new Label() { ID = "BLabel2" };
               
                // prefix
                GridView = new GridView() { ID = "grdGridView", AutoGenerateColumns = false };
                
                TemplateField tf1 = new TemplateField();
                tf1.HeaderText = "Header template field 1";
                tf1.ItemTemplate = new Template1();

                GridView.Columns.Add(tf1);
                
                TemplateField tf2 = new TemplateField();
                tf2.HeaderText = "Header template field 2";
                tf2.ItemTemplate = new Template2();

                GridView.Columns.Add(tf2);

                ListBox = new ListBox() { ID = "ListBox" };
                
                RadioButton = new RadioButton() { ID = "RadioButton" };

                Panel = new Panel() { ID = "Panel" };
                Label3 = new Label() { ID = "Label3" };
                Panel.Controls.Add(Label3);
                Label4 = new Label() { ID = "Label4" };
                Panel.Controls.Add(Label4);

                Repeater = new Repeater() { ID = "Repeater" };
                Repeater.ItemTemplate = new Template3();
                Repeater.AlternatingItemTemplate = new Template3();
                
                CheckBoxList = new CheckBoxList() { ID = "CheckBoxList" };

                RadioButtonList = new RadioButtonList() { ID = "RadioButtonList" };
                RadioButtonList.Items.Add(new ListItem("", ""));
                RadioButtonList.Items.Add(new ListItem("Si", "true"));
                RadioButtonList.Items.Add(new ListItem("No", "false"));
                
                ListBox2 = new ListBox() { ID = "ListBox2" };
                ListBox2.SelectionMode = ListSelectionMode.Multiple;
                ListBox2.Items.Add(new ListItem("Red", "1"));
                ListBox2.Items.Add(new ListItem("Blue", "2"));
                ListBox2.Items.Add(new ListItem("Green", "3"));
                
                Controls.Add(TextBox);
                Controls.Add(HiddenField);
                Controls.Add(Label);
                Controls.Add(NullableHiddenField);
                Controls.Add(EnumHiddenField);
                Controls.Add(Label2);
                Controls.Add(GridView);
                Controls.Add(ListBox);
                Controls.Add(RadioButton);
                Controls.Add(Panel);
                Controls.Add(Repeater);
                Controls.Add(CheckBoxList);
                Controls.Add(RadioButtonList);
                Controls.Add(ListBox2);
            }

            public void Setup()
            {   
                TextBox.Text = "03/03/2013";
                HiddenField.Value = "1";
                Label.Text = "Nicola";
                NullableHiddenField.Value = "2";
                // EnumHiddenField.Value = "Due"; // Enum name
                EnumHiddenField.Value = "2"; // Enum value
                Label2.Text = "Tuveri"; 
               
                GridView.AutoGenerateColumns = false;
                var gridViewDataSource = new List<C>() { 
                    new C { InnerHiddenField = 678, InnerLabel = "Cagliari0", InnerTextBox = new DateTime(2000, 1, 1) }, 
                    new C { InnerHiddenField = 679 , InnerLabel = "Cagliari1", InnerTextBox = new DateTime(2001, 1, 1) } 
                };
                GridView.DataSource = gridViewDataSource;
                GridView.DataBind();
                GridView.DataSource = null;

                ((Label) GridView.Rows[0].Cells[0].FindControl("InnerLabel")).Text = gridViewDataSource[0].InnerLabel;
                ((HiddenField) GridView.Rows[0].Cells[0].FindControl("InnerHiddenField")).Value = 
                    gridViewDataSource[0].InnerHiddenField.ToString();
                ((TextBox) GridView.Rows[0].Cells[1].FindControl("InnerTextBox")).Text = 
                    gridViewDataSource[0].InnerTextBox.ToString();

                ((Label) GridView.Rows[1].Cells[0].FindControl("InnerLabel")).Text = gridViewDataSource[1].InnerLabel;
                ((HiddenField) GridView.Rows[1].Cells[0].FindControl("InnerHiddenField")).Value = 
                    gridViewDataSource[1].InnerHiddenField.ToString();
                ((TextBox) GridView.Rows[1].Cells[1].FindControl("InnerTextBox")).Text = 
                    gridViewDataSource[1].InnerTextBox.ToString();

                ListBox.DataSource = new List<D>() { 
                    new D { Id = 10, Descrizione = "Dieci" }, 
                    new D { Id = 11, Descrizione = "Undici" } 
                };
                ListBox.DataValueField = "Id";
                ListBox.DataTextField = "Descrizione";
                ListBox.SelectionMode = ListSelectionMode.Multiple;
                ListBox.DataBind();
                ListBox.SelectedValue = "11";
                ListBox.DataSource = null;

                RadioButton.Checked = true;

                Label3.Text = "12,34";
                Label4.Text = "56,78";

                var repeaterDataSource = new List<G>[] { 
                    null, null, null
                };
                
                var dropDownDataSource = new List<G>() {
                    new G() { Text = "Red", Value = 1 },
                    new G() { Text = "Green", Value = 2 },
                    new G() { Text = "Blue", Value = 3 }
                };

                Repeater.DataSource = repeaterDataSource;
                Repeater.DataBind();

                foreach (var ri in Repeater.Items.Cast<RepeaterItem>())
                {
                    if(ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
                    {   
                        var dd = (DropDownList) ri.FindControl("InnerDropDown");
                        dd.DataSource = dropDownDataSource;
                        dd.DataBind();
                    }
                }

                CheckBoxList.DataSource = new List<D>() { 
                    new D { Id = 77, Descrizione = "Settantasette" }, 
                    new D { Id = 88, Descrizione = "Ottantotto" }, 
                    new D { Id = 99, Descrizione = "Novantanove" } 
                };
                CheckBoxList.DataValueField = "Id";
                CheckBoxList.DataTextField = "Descrizione";
                CheckBoxList.DataBind();
                CheckBoxList.Items[1].Selected = true;
                CheckBoxList.Items[2].Selected = true;
                CheckBoxList.DataSource = null;

                RadioButtonList.Items[1].Selected = true;

                ListBox2.Items[1].Selected = true;
                ListBox2.Items[2].Selected = true;
            }
        }

        class A
        {
            public DateTime TextBox { get; set; }
            private int HiddenField { get; set; }
            protected string Label { get; set; }
            public int? NullableHiddenField { get; set; }
            public E EnumHiddenField{ get; set; }

            public B B { get; set; }

            public List<C> GridView { get; set; }
            
            public List<D> ListBox { get; set; } 
            // public int ListBoxSelected { get; set; } // ListSelectionMode.Single
            public int[] ListBoxSelected { get; set; } // ListSelectionMode.Multiple

            public bool RadioButton { get; set; }

            public Dictionary<string, double> Panel { get; set; }

            public List<G>[] Repeater { get; set; }

            public D[] CheckBoxList { get; set; } 
            public List<int> CheckBoxListSelected { get; set; }

            public bool? RadioButtonList {  get; set; } 
            
            public int[] ListBox2 { get; set; }

            public void Setup() 
            {
                TextBox = new DateTime(2013, 3, 3);
                HiddenField = 1;
                B = new B();
                B.Label2 = "Tuveri";
                NullableHiddenField = 2;
                EnumHiddenField = E.Due;
                
                GridView = new List<C>() { 
                    new C { InnerHiddenField = 678, InnerLabel = "Cagliari0", InnerTextBox = new DateTime(2000, 1, 1) }, 
                    new C { InnerHiddenField = 679 , InnerLabel = "Cagliari1", InnerTextBox = new DateTime(2001, 1, 1) } 
                };

                ListBox = new List<D>() { 
                    new D { Id = 10, Descrizione = "Dieci" }, 
                    new D { Id = 11, Descrizione = "Undici" }
                };

                ListBoxSelected = new int[] { 11 };

                RadioButton = true;
            
                Panel = new Dictionary<string,double>();
                Panel["Label3"] = 12.34;
                Panel["Label4"] = 56.78;

                var dropDown = new List<G>() {
                    new G() { Text = "Red", Value = 1 },
                    new G() { Text = "Green", Value = 2 },
                    new G() { Text = "Blue", Value = 3 }
                };

                var repeater = new List<G>[] {
                    dropDown, dropDown, dropDown 
                };

                Repeater = repeater;

                CheckBoxList = new D[] { 
                    new D { Id = 77, Descrizione = "Settantasette" }, 
                    new D { Id = 88, Descrizione = "Ottantotto" }, 
                    new D { Id = 99, Descrizione = "Novantanove" } 
                };

                CheckBoxListSelected = new List<int> { 66, 88, 99 };

                RadioButtonList = false;

                ListBox2 = new int[] { 1, 3 };
            }
        }

        class B 
        {
            public string Label2 { get; set; }
        }

        class C 
        {
            public string InnerLabel { get; set; }
            public int InnerHiddenField { get; set; }
            public DateTime InnerTextBox { get; set; }
        }
        
        class D 
        {
            public int Id { get; set; }
            public string Descrizione { get; set; }
        }

        enum E
        {
            Uno = 1,
            Due,
            Tre
        }

        class F
        {
            public string Numero { get; set; }
            public string Numero2 { get; set; }
        }

        public class G
        {
            public string Text { get; set; }
            public long Value { get; set; }
        }


        [Test]
        public void MapWebControlToObjectTest()
        {
            Page1 p1 = new Page1();
            p1.Setup();
            Mapper m = new Mapper();
            A a = m.Map<Page1, A>(p1);

            Assert.AreEqual(new DateTime(2013, 3, 3), a.TextBox);
            Assert.AreEqual(1, typeof(A).GetProperty("HiddenField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(a, null));
            Assert.AreEqual("Tuveri", a.B.Label2);
            Assert.AreEqual(2, typeof(A).GetProperty("NullableHiddenField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(a, null));
            Assert.AreEqual(E.Due, a.EnumHiddenField);

            Assert.AreEqual(2, a.GridView.Count);
            Assert.AreEqual(678, a.GridView[0].InnerHiddenField);
            Assert.AreEqual("Cagliari0", a.GridView[0].InnerLabel);
            Assert.AreEqual(new DateTime(2000, 1, 1), a.GridView[0].InnerTextBox);
            Assert.AreEqual(679, a.GridView[1].InnerHiddenField);
            Assert.AreEqual("Cagliari1", a.GridView[1].InnerLabel);
            Assert.AreEqual(new DateTime(2001, 1, 1), a.GridView[1].InnerTextBox);

            Assert.AreEqual(2, a.ListBox.Count);
            Assert.AreEqual(10, a.ListBox[0].Id);
            Assert.AreEqual("Dieci", a.ListBox[0].Descrizione);
            Assert.AreEqual(11, a.ListBox[1].Id);
            Assert.AreEqual("Undici", a.ListBox[1].Descrizione);
            Assert.AreEqual(1, a.ListBoxSelected.Length);
            Assert.AreEqual(11, a.ListBoxSelected[0]);

            Assert.AreEqual(true, a.RadioButton);

            Assert.AreEqual(12.34, a.Panel["Label3"]);
            Assert.AreEqual(56.78, a.Panel["Label4"]);

            Assert.AreEqual(3, a.Repeater.Length);
            Assert.AreEqual(3, a.Repeater[0].Count);
            Assert.AreEqual("Red", a.Repeater[0][0].Text);
            Assert.AreEqual(2, a.Repeater[0][1].Value);

            var cs = m.Map<GridView, List<C>>(p1.GridView);

            Assert.AreEqual(2, cs.Count);
            Assert.AreEqual(678, cs[0].InnerHiddenField);
            Assert.AreEqual("Cagliari0", cs[0].InnerLabel);
            Assert.AreEqual(new DateTime(2000, 1, 1), cs[0].InnerTextBox);
            Assert.AreEqual(679, cs[1].InnerHiddenField);
            Assert.AreEqual("Cagliari1", cs[1].InnerLabel);
            Assert.AreEqual(new DateTime(2001, 1, 1), cs[1].InnerTextBox);

            var c = m.Map<GridViewRow, C>(p1.GridView.Rows[0]);
            Assert.AreEqual(678, c.InnerHiddenField);
            Assert.AreEqual("Cagliari0", c.InnerLabel);
            Assert.AreEqual(new DateTime(2000, 1, 1), c.InnerTextBox);

            Assert.AreEqual(3, a.CheckBoxList.Length);
            Assert.AreEqual(77, a.CheckBoxList[0].Id);
            Assert.AreEqual("Settantasette", a.CheckBoxList[0].Descrizione);
            Assert.AreEqual(88, a.CheckBoxList[1].Id);
            Assert.AreEqual("Ottantotto", a.CheckBoxList[1].Descrizione);
            Assert.AreEqual(99, a.CheckBoxList[2].Id);
            Assert.AreEqual("Novantanove", a.CheckBoxList[2].Descrizione);

            Assert.AreEqual(2, a.CheckBoxListSelected.Count);
            Assert.AreEqual(88, a.CheckBoxListSelected[0]);
            Assert.AreEqual(99, a.CheckBoxListSelected[1]);

            Assert.AreEqual(true, a.RadioButtonList);

            Assert.AreEqual(2, a.ListBox2.Length);
            Assert.AreEqual(2, a.ListBox2[0]);
            Assert.AreEqual(3, a.ListBox2[1]);
        }

        [Test]
        public void MapDictionaryToObject() 
        {
            var dict = new Dictionary<string, object>() { 
                { "Numero", "Uno" }, { "Numero2", "Due" }
            };
            Mapper m = new Mapper();
            var f = m.Map<F>(dict, dict.GetType());

            Assert.AreEqual("Uno", f.Numero);
            Assert.AreEqual("Due", f.Numero2);

            var dict2 = m.Map<Dictionary<string, string>>(f, typeof(F));

            Assert.AreEqual("Uno", dict2["Numero"]);
            Assert.AreEqual("Due", dict2["Numero2"]);
        }

        [Test]
        public void MapObjectToWebControlTest()
        {
            A a = new A();
            a.Setup();
            Page1 p1 = new Page1();

            p1.ListBox.DataValueField = "Id";
            p1.ListBox.DataTextField = "Descrizione";
            p1.ListBox.SelectionMode = ListSelectionMode.Multiple;
            
            p1.CheckBoxList.DataValueField = "Id";
            p1.CheckBoxList.DataTextField = "Descrizione";
            
            Mapper m = new Mapper();
            m.Map<A, Page1>(a, p1);

            Assert.AreEqual("03/03/2013", p1.TextBox.Text);
            Assert.AreEqual("1", ((HiddenField) p1.FindControl("hdnHiddenField")).Value);
            Assert.AreEqual("Tuveri", p1.Label2.Text);
            Assert.AreEqual("2", p1.NullableHiddenField.Value);
            Assert.AreEqual("Due", p1.EnumHiddenField.Value);

            Assert.AreEqual(2, p1.GridView.Rows.Count);
            Assert.AreEqual("678", ((HiddenField) p1.GridView.Rows[0].FindControl("InnerHiddenField")).Value);
            Assert.AreEqual("Cagliari0", ((Label) p1.GridView.Rows[0].FindControl("InnerLabel")).Text);
            Assert.AreEqual("01/01/2000", ((TextBox) p1.GridView.Rows[0].FindControl("InnerTextBox")).Text);
            Assert.AreEqual("679", ((HiddenField) p1.GridView.Rows[1].FindControl("InnerHiddenField")).Value);
            Assert.AreEqual("Cagliari1", ((Label) p1.GridView.Rows[1].FindControl("InnerLabel")).Text);
            Assert.AreEqual("01/01/2001", ((TextBox) p1.GridView.Rows[1].FindControl("InnerTextBox")).Text);
            
            Assert.AreEqual(2, p1.ListBox.Items.Count);
            Assert.AreEqual("10", p1.ListBox.Items[0].Value);
            Assert.AreEqual("Dieci", p1.ListBox.Items[0].Text);
            Assert.AreEqual("11", p1.ListBox.Items[1].Value);
            Assert.AreEqual("Undici", p1.ListBox.Items[1].Text);
            
            Assert.AreEqual(false, p1.ListBox.Items[0].Selected);
            Assert.AreEqual(true, p1.ListBox.Items[1].Selected);
            
            Assert.AreEqual(true, p1.RadioButton.Checked);

            Assert.AreEqual("12,34", p1.Label3.Text);
            Assert.AreEqual("56,78", p1.Label4.Text);

            Assert.AreEqual(3, p1.Repeater.Items.Count);
            Assert.AreEqual(3, ((DropDownList) p1.Repeater.Items[0].FindControl("InnerDropDown")).Items.Count);
            Assert.AreEqual("Green", ((DropDownList) p1.Repeater.Items[0].FindControl("InnerDropDown")).Items[1].Text);
            Assert.AreEqual("3", ((DropDownList) p1.Repeater.Items[0].FindControl("InnerDropDown")).Items[2].Value);

            Assert.AreEqual(3, p1.CheckBoxList.Items.Count);
            Assert.IsNull(p1.CheckBoxList.Items.FindByValue("66"));
            Assert.IsFalse(p1.CheckBoxList.Items.FindByValue("77").Selected);
            Assert.IsTrue(p1.CheckBoxList.Items.FindByValue("88").Selected);
            Assert.IsTrue(p1.CheckBoxList.Items.FindByValue("99").Selected);

            Assert.AreEqual("false", p1.RadioButtonList.SelectedValue);
            
            Assert.IsTrue(p1.ListBox2.Items.FindByValue("1").Selected);
            Assert.IsFalse(p1.ListBox2.Items.FindByValue("2").Selected);
            Assert.IsTrue(p1.ListBox2.Items.FindByValue("3").Selected);
        }

        struct Geo 
        {
            public float Latitude { get; set; }
            public float Longitude { get; set; }
        }

        class GeoString
        {
            public string Value { get; set; }
        }

        class GeoStruct
        {
            public Geo Value { get; set; }
        }

        class GeoTypeConverter : ITypeConverter
        {
            public bool  CanConvert(Type sourceType, Type targetType)
            {
 	            return (sourceType == typeof(string) && targetType == typeof(Geo)) ||
 	                (sourceType == typeof(Geo) && targetType == typeof(string));
            }

            public object Convert(object source, Type targetType)
            {
 	            if(targetType == typeof(string))
                {
                    Geo geo = (Geo) source;
                    return string.Format(CultureInfo.InvariantCulture, "{0},{1}", geo.Latitude, geo.Longitude);
                }
                
                string[] geoString = ((string) source).Split(',');
                return new Geo() { 
                    Latitude = float.Parse(geoString[0], CultureInfo.InvariantCulture), 
                    Longitude = float.Parse(geoString[1], CultureInfo.InvariantCulture) 
                };
            }
        }


        [Test]
        public void MapCustomTypeConverterTest()
        {
            Mapper m = new Mapper();
            m.Config.TypeConverters.Insert(0, new GeoTypeConverter());
            
            string str = m.Map<Geo, string>(new Geo() { Latitude = -32.1f, Longitude = 987.654f });
            Assert.AreEqual("-32.1,987.654", str);

            Geo geo = m.Map<string, Geo>("11.22,33.44");
            Assert.AreEqual(11.22f, geo.Latitude);
            Assert.AreEqual(33.44f, geo.Longitude);

            GeoString geoString = new GeoString() { Value = "+123.45,-67.98" };
            GeoStruct geoStruct = m.Map<GeoString, GeoStruct>(geoString);
            
            Assert.AreEqual(123.45f, geoStruct.Value.Latitude);
            Assert.AreEqual(-67.98f, geoStruct.Value.Longitude);

            geoStruct = new GeoStruct() { Value = new Geo() { Latitude = -32.1f, Longitude = 987.654f }};
            geoString = m.Map<GeoStruct, GeoString>(geoStruct);
            Assert.AreEqual("-32.1,987.654", geoString.Value);

        }

        [Test]
        public void NativeTypeConverterTest()
        {
            var converter = new NativeTypeConverter();
            var intValue = converter.Convert((long) 1, typeof(int));

            Assert.AreEqual(typeof(int), intValue.GetType());
            Assert.AreEqual((int) 1, intValue);

            intValue = converter.Convert((float) 2.0, typeof(int));

            Assert.AreEqual(typeof(int), intValue.GetType());
            Assert.AreEqual((int) 2.0, intValue);
        }

        public class ClassIInt
        {
            public int I { get; set; }
        }
        public class ClassIString
        {
            public string I { get; set; }
        }
        

        [Test]
        public void MapperErrorTest()
        {
            Mapper m = new Mapper();
            var i = m.Map<string, int>("abc");
            Assert.AreEqual(1, m.Context.MappingErrors.Count);
            Assert.AreEqual("Il valore abc non è valido per il campo  di tipo intero.", m.Context.MappingErrors[0].ToString());

            var ci = m.Map<ClassIString, ClassIInt>(new ClassIString() { I = "abc" });
            Assert.AreEqual(1, m.Context.MappingErrors.Count);
            Assert.AreEqual("Il valore abc non è valido per il campo I di tipo intero.", m.Context.MappingErrors[0].ToString());
         
        }
    }
}
