using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Projekat.Models
{
    public class Pacijent
    {
        public string KorisnickoIme {  get; set; }
        public string Jmbg {  get; set; }   
        public string Sifra {  get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public string ElektronskaPosta { get; set; }
        public List<Termin> ZakazaniTermini { get; set; }
        public int Id {  get; set; }

        public bool IsDeleted { get; set; }

        public ZdravstveniKarton ZdravstveniKarton { get; set; }

        public Pacijent() { }

        public Pacijent(string korisnickoIme, string jmbg, string sifra, string ime, string prezime, DateTime datumRodjenja, string elektronskaPosta, int id)
        {
            KorisnickoIme = korisnickoIme;
            Jmbg = jmbg;
            Sifra = sifra;
            Ime = ime;
            Prezime = prezime;
            DatumRodjenja = datumRodjenja;
            ElektronskaPosta = elektronskaPosta;
            ZakazaniTermini = new List<Termin>();
            Id = id;
            IsDeleted = false;
        }

        public void DodajTermin(Termin termin)
        {
            ZakazaniTermini.Add(termin);
        }
    }
}