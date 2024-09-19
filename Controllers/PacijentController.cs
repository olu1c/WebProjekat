using Projekat.Helperi;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class PacijentController : Controller
    {
        // GET: Pacijent
        public ActionResult Index()
        {
            if (Session["auth"] != null)
            {
                Pacijent prijavljeniPacijent = Pacijenti.PacijentiList.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

            if (prijavljeniPacijent == null)
            {
                return HttpNotFound();
            }

            Session["FilteredTermini"] = Termini.ListaTermina;
            return View(Termini.ListaTermina);

            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult ZakaziTermin(int terminId)
        {
            Pacijent prijavljeniPacijent = Pacijenti.PacijentiList.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

            // SESIJA
            if (Session["auth"] == null || ((PomocnaSesija)Session["auth"]).Podela != "P")
            {
                return RedirectToAction("Index", "Home");
            }

            if (prijavljeniPacijent == null)
            {
                return HttpNotFound();
            }

            Termin termin = Termini.ListaTermina.FirstOrDefault(t => t.Id == terminId && t.StatusTermina == StatusTermina.Slobodan);

            if (termin == null)
            {
                return HttpNotFound("Termin nije pronadjen ili je vec zauzet.");
            }

            if (termin.DatumZakazanogTermina < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Ne mozete zakazati termin ciji je datum istekao.";
                return View("Index", Termini.ListaTermina);
            }

            termin.StatusTermina = StatusTermina.Zauzet;
            termin.Pacijent = prijavljeniPacijent;

            if (prijavljeniPacijent.ZdravstveniKarton == null)
            {
                prijavljeniPacijent.ZdravstveniKarton = new ZdravstveniKarton(new List<Termin>(), prijavljeniPacijent);
            }
            prijavljeniPacijent.ZdravstveniKarton.DodajTermin(termin);

            prijavljeniPacijent.DodajTermin(termin);

            SaveToFile(prijavljeniPacijent.ZdravstveniKarton, termin);

            return RedirectToAction("Index");
        }

        private void SaveToFile(ZdravstveniKarton zdravstveniKarton, Termin termin)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Termini.txt");
            zdravstveniKarton.Pacijent = Pacijenti.PacijentiList.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

            //string pacijentData = $"{termin.DanTermina},{termin.DatumZakazanogTermina},{Parser(termin.StatusTermina)},{((PomocnaSesija)Session["auth"]).Id},{termin.OpisTerapije},{termin.Id}";

            //string pacijentData = $"{termin.DanTermina},{termin.DatumZakazanogTermina},{termin.StatusTermina},{termin.Lekar.Id},{termin.OpisTerapije},{termin.Id}";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach(Termin t in Termini.ListaTermina)
                {
                    writer.WriteLine(t);
                }
                
            }
        }

        [HttpPost]
        public ActionResult Sortiranje(string parametarSortiranja, string redosledSortiranja)
        {
            var filteredTermini = Session["FilteredTermini"] as List<Termin>;
            if (filteredTermini == null)
            {
                return RedirectToAction("Index");
            }

            switch (parametarSortiranja)
            {
                case "Lekar":
                    filteredTermini = redosledSortiranja == "asc" ?
                                     filteredTermini.OrderBy(t => t.Lekar.KorisnickoIme).ToList() :
                                     filteredTermini.OrderByDescending(t => t.Lekar.KorisnickoIme).ToList();
                    break;
                case "Datum":
                    filteredTermini = redosledSortiranja == "asc" ?
                                     filteredTermini.OrderBy(t => t.DatumZakazanogTermina).ToList() :
                                     filteredTermini.OrderByDescending(t => t.DatumZakazanogTermina).ToList();
                    break;
                case "Vreme":
                    filteredTermini = redosledSortiranja == "asc" ?
                                     filteredTermini.OrderBy(t => t.DatumZakazanogTermina.TimeOfDay).ToList() :
                                     filteredTermini.OrderByDescending(t => t.DatumZakazanogTermina.TimeOfDay).ToList();
                    break;
                default:
                    break;
            }

            return View("Index", filteredTermini);
        }

        public ActionResult ResetovanjeSortiranja()
        {
            var filteredTermini = Session["FilteredTermini"] as List<Termin>;
            if (filteredTermini == null)
            {
                return RedirectToAction("Index");
            }

            Pacijent prijavljeniPacijent = Pacijenti.PacijentiList.FirstOrDefault(p => p.Id == ((PomocnaSesija)Session["auth"]).Id);
            Tuple<Pacijent, List<Termin>> tuple = new Tuple<Pacijent, List<Termin>>(prijavljeniPacijent, filteredTermini);
            return View("Index", tuple);
        }

        [HttpPost]
        public ActionResult Filtriranje(string parametar, string vrednost)
        {
            var termini = Termini.ListaTermina; 

            switch (parametar)
            {
                case "Lekar":
                    termini = termini.Where(t => t.Lekar.KorisnickoIme.Contains(vrednost)).ToList();
                    break;
                case "Datum":
                    if (DateTime.TryParse(vrednost, out DateTime datum))
                    {
                        termini = termini.Where(t => t.DatumZakazanogTermina.Date == datum.Date).ToList();
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Neispravan format datuma.";
                    }
                    break;
                default:
                    ViewBag.ErrorMessage = "Nepoznat parametar za filtriranje.";
                    break;
            }

            Session["FilteredTermini"] = termini;

            return View("Index", termini);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult PregledTerapija()
        {
            Pacijent prijavljeniPacijent = Pacijenti.PacijentiList.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

            if (prijavljeniPacijent == null)
            {
                return HttpNotFound();
            }

            var terminiSaTerapijom = Termini.ListaTermina.Where(t => t.Pacijent.Id == prijavljeniPacijent.Id && !string.IsNullOrEmpty(t.OpisTerapije)).ToList();

            return View(terminiSaTerapijom);
        }
    }

}
