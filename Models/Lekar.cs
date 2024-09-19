using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Projekat.Models
{
    public class Lekar
    {
        public string KorisnickoIme { get; set; }
        public string Sifra {  get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public string ElektronskaPosta { get; set; }
        public List<Termin> Termini {get; set; }
        public List<Termin> Zakazani {  get; set; }
        public List<Termin> Slobodni {  get; set; }

        public List<string> DaniUNedelji { get; set; }

        public int Id { get; set; }

        public Lekar()
        {

        }

        public Lekar(string korisnickoIme, string sifra, string ime, string prezime, DateTime datumRodjenja, string elektronskaPosta, int id, List<string> daniUNedelji)
        {
            KorisnickoIme = korisnickoIme;
            Sifra = sifra;
            Ime = ime;
            Prezime = prezime;
            DatumRodjenja = datumRodjenja;
            ElektronskaPosta = elektronskaPosta;
            //Termini = termini;
            Zakazani = new List<Termin>();
            Slobodni = new List<Termin>();
            Id = id;
            DaniUNedelji = daniUNedelji ?? new List<string>();
            Termini = new List<Termin>();   
        }
    }
}