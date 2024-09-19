using Projekat.Helperi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Projekat.Models
{
    public enum StatusTermina{
        Slobodan,
        Zauzet
    };

    public class Termin
    {
        public Lekar Lekar { get; set; }
        public StatusTermina StatusTermina { get; set; }
        public DateTime DatumZakazanogTermina { get; set; }
        
        public string DanTermina { get; set; }
        public string OpisTerapije { get; set; }

        public int Id { get; set; }
        public Pacijent Pacijent { get; set; }

        public Termin() { }

        public Termin(Lekar lekar, StatusTermina statusTermina, DateTime datumZakazanogTermina, string opisTerapije, string danTermina, Pacijent p)
        {
            Lekar = lekar;
            StatusTermina = statusTermina;
            DatumZakazanogTermina = datumZakazanogTermina;
            OpisTerapije = opisTerapije;
            DanTermina = danTermina;
            Pacijent = p;
            Id = nextId++;
        }

        public Termin(Lekar lekar, StatusTermina statusTermina, DateTime datumZakazanogTermina, string opisTerapije, string danTermina, Pacijent p, int id)
        {
            Lekar = lekar;
            StatusTermina = statusTermina;
            DatumZakazanogTermina = datumZakazanogTermina;
            OpisTerapije = opisTerapije;
            DanTermina = danTermina;
            Pacijent = p;
            Id = id;
        }

        public override string ToString()
        {
            return $"{this.DanTermina},{this.DatumZakazanogTermina},{this.StatusTermina},{this.Lekar.Id},{this.OpisTerapije},{this.Id},{(Pacijent == null ? -1 : this.Pacijent.Id)}";
        }

        public static int nextId = 1;
    }
}