using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Projekat.Models
{
    public class ZdravstveniKarton
    {
        public List<Termin> ListaTermina {  get; set; }
        public Pacijent Pacijent { get; set; }

        public ZdravstveniKarton() { }

        public ZdravstveniKarton(List<Termin> listaTermina, Pacijent pacijent)
        {
            ListaTermina = new List<Termin>();
            Pacijent = pacijent;
        }

        public void DodajTermin(Termin termin)
        {
            ListaTermina.Add(termin);
        }
    }
}