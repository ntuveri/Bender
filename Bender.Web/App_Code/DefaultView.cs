using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bender.Web
{
    public class DefaultView
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public Panel Pannello { get; set; }
        public List<Gender> Sesso { get; set; } 
        public int ColorsSelected { get; set; } 
        public List<Patente> Patenti { get; set; } 
        public List<string> PatentiSelected { get; set; } 
    }
}