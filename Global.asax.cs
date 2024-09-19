using Projekat.Helperi;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Projekat
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ProcitajPacijente();
            ProcitajLekare();
            ProcitajAdministratore();
            ProcitajTermine();

        }

        public static void ProcitajPacijente()
        {
            string txtFolder = HttpContext.Current.Server.MapPath("~/App_Data/Pacijenti.txt");
            string[] lines = File.ReadAllLines(txtFolder);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 9)
                {
                    //korisnik1,0000000000001,sifra1,Pera,Peric,01/01/1980,pera.peric@example.com,1
                    string korisnickoIme = parts[0];
                    string jmbg = parts[1];
                    string lozinka = parts[2];
                    string ime = parts[3];
                    string prezime = parts[4];
                    //DateTime datumRodjenja = DateTime.Parse(parts[5]);
                    // DateTime datumRodjenja = DateTime.Parse(parts[5]);
                    string format = "M/d/yyyy";
                    string d = parts[5].Split(' ')[0];
                    DateTime datumRodjenja = DateTime.ParseExact(d, format, System.Globalization.CultureInfo.InvariantCulture);
                    /*bool uspeh = DateTime.TryParseExact(parts[5], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out datumRodjenja) ||
                        DateTime.TryParseExact(parts[5], "MM/dd/yyyy HH:mm:ss ", CultureInfo.InvariantCulture, DateTimeStyles.None, out datumRodjenja);
                    if (!uspeh)
                    {
                        // Obradite slučaj kada datum nije validan, npr. preskočite unos ili prijavite grešku
                        Console.WriteLine($"Neuspešno parsiranje datuma za korisnika {korisnickoIme}. Vrednost datuma: {parts[5]}");
                        continue; // preskočite ovaj unos
                    }
                    */
                    string email = parts[6];
                    int id = int.Parse(parts[7]);
                    string obrisan = parts[8];

                    Pacijent pacijenti = new Pacijent(korisnickoIme, jmbg, lozinka, ime, prezime, datumRodjenja, email, id);
                    pacijenti.IsDeleted = obrisan.Equals("True") ? true : false;
                    Pacijenti.PacijentiList.Add(pacijenti);
                    //Pacijenti.PacijentiList.Add(pacijenti);
                    int i = 0;
                }
            }

        }

        public static void ProcitajLekare()
        {
            string txtFolder = HttpContext.Current.Server.MapPath("~/App_Data/Lekari.txt");
            string[] lines = File.ReadAllLines(txtFolder);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                
                    //korisnik1,sifra1,Pera,Peric,01/01/1970,pera.peric@example.com,1
                    string korisnickoIme = parts[0];
                    string lozinka = parts[1];
                    string ime = parts[2];
                    string prezime = parts[3];
                    DateTime datumRodjenja = DateTime.ParseExact(parts[4], "dd/MM/yyyy", null);
                    string email = parts[5];
                    int id = int.Parse(parts[6]);
                    //List<string> daniUNedelji = parts[7].Split(',').ToList();
                    List<string> daniUNedelji = parts[7].Split('|').ToList();

                     Lekar lekari = new Lekar(korisnickoIme, lozinka, ime, prezime, datumRodjenja, email, id, daniUNedelji);
                    //Pacijenti.PacijentiList.Add(pacijenti);
                    //Pacijenti.PacijentiList.Add(pacijenti);
                    Lekari.LekariLista.Add(lekari); 

                
            }

        }

        public static void ProcitajAdministratore()
        {
            string txtFolder = HttpContext.Current.Server.MapPath("~/App_Data/Administratori.txt");
            string[] lines = File.ReadAllLines(txtFolder);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 6)
                {
                    //admin1,sifra1,Marko,Markovic,01/01/1980,1
                    string korisnickoIme = parts[0];
                    string lozinka = parts[1];
                    string ime = parts[2];
                    string prezime = parts[3];
                    DateTime datumRodjenja = DateTime.Parse(parts[4]);
                    int id = int.Parse(parts[5]);

                    Administrator administratori = new Administrator(korisnickoIme, lozinka, ime, prezime, datumRodjenja, id);
                    Administratori.AdministratoriLista.Add(administratori);
                    //Pacijenti.PacijentiList.Add(pacijenti);

                }
            }

        }

        public static void ProcitajTermine()
        {
            string txtFolder = HttpContext.Current.Server.MapPath("~/App_Data/Termini.txt");
            string[] lines = File.ReadAllLines(txtFolder);
            int k = 0;
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');

                string dan = parts[0];
                //DateTime datumTermina = DateTime.Parse(parts[1]);
                DateTime datumTermina;
                bool uspeh = DateTime.TryParseExact(parts[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out datumTermina);

                //bool uspeh = DateTime.TryParseExact(parts[1], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out datumTermina);
                if (!uspeh)
                {
                    // Obradite slučaj kada datum nije validan, npr. preskočite unos ili prijavite grešku
                    Console.WriteLine($"Neuspešno parsiranje datuma: {parts[1]}");
                    continue; // preskočite ovaj unos
                }
                StatusTermina status = StatusTermina(parts[2]);
                int lekarId = int.Parse(parts[3]);

                Lekar lekar = null;
                foreach(Lekar l in Lekari.LekariLista)
                {
                    if(lekarId == l.Id)
                    {
                        lekar = l; break;
                    }
                }
                string opisTerapije = parts[4];

                int terminId = int.Parse(parts[5]);

                int pacijentId = int.Parse(parts[6]);
                Pacijent pacijent = null;
                foreach (Pacijent p in Pacijenti.PacijentiList)
                {
                    if (pacijentId == p.Id)
                    {
                        pacijent = p; break;
                    }
                }

                Termin termin = new Termin(lekar, status, datumTermina, opisTerapije, dan, pacijent, terminId);
                Termini.ListaTermina.Add(termin);
                k++;
                
            }
            Termin.nextId = k+1;

        }

        private static StatusTermina StatusTermina(string status) 
        {
            if(status.Equals("Slobodan"))
            {
                return Models.StatusTermina.Slobodan;
            }
            else
            {
                return Models.StatusTermina.Zauzet;
            }
               
        }


    }
}
