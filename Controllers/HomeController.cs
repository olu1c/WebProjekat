using Projekat.Helperi;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["pomocnaSesija"] == null)
            {
                return View();
            }

            PomocnaSesija pomocnaSesija = Session["pomocnaSesija"] as PomocnaSesija;
            if (pomocnaSesija != null)
            {
                switch (pomocnaSesija.Podela)
                {
                    case "A":
                        return RedirectToAction("index", "Administrator");
                    case "L":
                        return RedirectToAction("index", "Lekar");
                    case "P":
                        return RedirectToAction("index", "Pacijent");
                    default:
                        return RedirectToAction("index", "Home");
                }
            }

            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Prijava()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Prijava(string korisnickoIme, string lozinka)
        {
            if (string.IsNullOrEmpty(korisnickoIme) || string.IsNullOrEmpty(lozinka))
            {
                ViewBag.ErrorMessage = "Unesite korisnicko ime i lozinku.";
                return View("Index");
            }

            Pacijent pacijent = ProveriPacijenta(korisnickoIme, lozinka);
            Lekar lekar = ProveriLekara(korisnickoIme, lozinka);
            Administrator admin = ProveriAdministratora(korisnickoIme, lozinka);

            if (admin != null)
            {
                int id = -1;
                foreach (Administrator l in Administratori.AdministratoriLista)
                {
                    if (l.KorisnickoIme.Equals(korisnickoIme))
                    {
                        id = l.Id;
                    }

                }

                Session["auth"] = new PomocnaSesija { UserName = korisnickoIme, Podela = "A", Id = id };
                return RedirectToAction("Index", "Administrator");
            }
            else if (lekar != null)
            {
                int id = -1;
                foreach(Lekar l in Lekari.LekariLista)
                {
                    if (l.KorisnickoIme.Equals(korisnickoIme))
                    {
                        id = l.Id;
                    }
                    
                }

                Session["auth"] = new PomocnaSesija { UserName = korisnickoIme, Podela = "L", Id = id};
                return RedirectToAction("Index", "Lekar");
            }
            else if (pacijent != null && pacijent.IsDeleted != true)
            {
                int id = -1;
                foreach (Pacijent l in Pacijenti.PacijentiList)
                {
                    if (l.KorisnickoIme.Equals(korisnickoIme))
                    {
                        id = l.Id;
                    }

                }

                Session["auth"] = new PomocnaSesija { UserName = korisnickoIme, Podela = "P", Id = id };
                return RedirectToAction("Index", "Pacijent");
            }
            else
            {
                ViewBag.ErrorMessage = "Pogresno korisnicko ime ili lozinka.";
                return View("Index");
            }
        }

        private Pacijent ProveriPacijenta(string korisnickoIme, string lozinka)
        {
            foreach (var pacijent in Pacijenti.PacijentiList)
            {
                if (pacijent.KorisnickoIme == korisnickoIme && pacijent.Sifra == lozinka && pacijent.IsDeleted == false)
                {
                    return pacijent;
                }
            }
            return null;
        }

        private Lekar ProveriLekara(string korisnickoIme, string lozinka)
        {
            foreach (var lekar in Lekari.LekariLista)
            {
                if (lekar.KorisnickoIme == korisnickoIme && lekar.Sifra == lozinka)
                {
                    return lekar;
                }
            }
            return null;
        }

        private Administrator ProveriAdministratora(string korisnickoIme, string lozinka)
        {
            foreach (var admin in Administratori.AdministratoriLista)
            {
                if (admin.KorisnickoIme == korisnickoIme && admin.Sifra == lozinka)
                {
                    return admin;
                }
            }
            return null;
        }

        public ActionResult Odjava()
        {
            Session["korisnik"] = null;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}