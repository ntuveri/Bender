using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;


namespace Bender.Web
{
    public partial class Default : System.Web.UI.Page
    {
        DefaultView LoadData()
        {
            DefaultView dv = new DefaultView();
            dv.Pannello = new Panel() { 
                Albums = new Album[] {
                new Album() { Id = 10, Nome = "Capitol", Autore = "The Beatles", Data = new DateTime(1968, 1, 1) },
                new Album() { Id = 11, Nome = "Columbia", Autore = "Bob Dylan", Data = new DateTime(1966, 2, 2) }
                }
            };
        
            dv.Sesso = new List<Gender>() {  
                new Gender() { Id = "F", Descrizione = "Femminile" },
                new Gender() { Id = "M", Descrizione = "Maschile" },
            };

            dv.ColorsSelected = 2;
            dv.Id = Guid.NewGuid();
            dv.Nome = "Nicola";

            dv.Patenti = new List<Patente>() {  
                new Patente() { Id = "A", Descrizione = "Patente A" },
                new Patente() { Id = "B", Descrizione = "Patente B" },
                new Patente() { Id = "C", Descrizione = "Patente C" },
            };

            dv.PatentiSelected = new List<string>() { "C", "B" };

            return dv;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DefaultView dv = LoadData();
                Mapper m = new Mapper();
                m.Map(dv, this);


                // Print();
            }
        }

        void Print()
        {
            Response.Write("Properties" + "<br/>");
            PrintProperties();

            Response.Write("Fields" + "<br/>");
            PrintFields();

            Response.Write("Controls" + "<br/>");
            PrintControls(this);
        }

        void PrintFields()
        {
            foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).
                Where(f => f.DeclaringType == typeof(Default)))
            {
                Response.Write(field + "<br/>");
            }
        }


        void PrintProperties()
        {
            foreach (var property in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).
                    Where(p => p.DeclaringType == typeof(Default)))
            {
                Response.Write(property + "<br/>");
            }

        }

        void PrintControls(Control c)
        {
            Response.Write(c.ID + "<br/>");
            if (c.HasControls())
            {
                foreach (Control cc in c.Controls)
                {
                    PrintControls(cc);
                }
            }
        }

        protected void btnSalva_Click(object sender, EventArgs e)
        {
            Mapper m = new Mapper();
            DefaultView dv = m.Map<Default, DefaultView>(this);
        }
    }
}