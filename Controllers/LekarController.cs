using Projekat.Helperi;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class LekarController : Controller
    {
        // GET: Lekar
        public ActionResult Index()
        {
            if (Session["auth"] != null)
            {

                Lekar prijavljeniLekar = Lekari.LekariLista.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

                if (prijavljeniLekar == null)
                {
                    return HttpNotFound();
                }

                Tuple<Lekar, List<Termin>> tuple = new Tuple<Lekar, List<Termin>>(prijavljeniLekar, Termini.ListaTermina);

                Session["FiltredTermini"] = Termini.ListaTermina;
                Session["SviTermini"] = Termini.ListaTermina;
                return View(tuple);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        

        [HttpPost]
        public ActionResult KreirajTermin(DateTime datum, string opis, string dani)
        {
            
            Lekar prijavljeniLekar = Lekari.LekariLista.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

            //SESIJA
            if (Session["auth"] == null || ((PomocnaSesija)Session["auth"]).Podela != "L")
            {
                return RedirectToAction("Index", "Home");
            }

            if (prijavljeniLekar == null)
            {
                return HttpNotFound();
            }

            if (datum < DateTime.Now)
            {
                ViewBag.ErrorMessage = "Ne mozete kreirati termin u proslosti.";
                return RedirectToAction("Index");
            }

            Termin noviTermin = new Termin
            {
                Lekar = prijavljeniLekar,
                StatusTermina = StatusTermina.Slobodan,
                DatumZakazanogTermina = datum,
                OpisTerapije = opis,
                DanTermina = dani,
                Id = Termin.nextId++,
            };


            prijavljeniLekar.Termini.Add(noviTermin);
            Termini.ListaTermina.Add(noviTermin);
            SaveToFile(noviTermin);

            return RedirectToAction("Index");
        }

        private void SaveToFile(Termin termin)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Termini.txt");

            //string pacijentData = $"{termin.DanTermina},{termin.DatumZakazanogTermina},{Parser(termin.StatusTermina)},{((PomocnaSesija)Session["auth"]).Id},{termin.OpisTerapije},{termin.Id}";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (Termin t in Termini.ListaTermina)
                {
                    writer.WriteLine(t);
                }
            }

        }

        private string Parser(StatusTermina statusTermina)
        {
            if(statusTermina.Equals(StatusTermina.Slobodan)) 
            {
                return "Slobodan";  
            }
            else if(statusTermina.Equals(StatusTermina.Zauzet)) 
            { 
                return "Zauzet";  
            }
            return "";
        }

        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult PrepisivanjeTerapije(int terminId, string opisTerapije)
        {
            if (Session["auth"] == null || ((PomocnaSesija)Session["auth"]).Podela != "L")
            {
                return RedirectToAction("Index", "Home");
            }

            Lekar prijavljeniLekar = Lekari.LekariLista.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);

            if (prijavljeniLekar == null)
            {
                return HttpNotFound();
            }

            Termin termin = Termini.ListaTermina.FirstOrDefault(t => t.Id == terminId && t.Lekar.Id == prijavljeniLekar.Id /*&& t.DatumZakazanogTermina >= DateTime.Now*/);

            if (termin == null)
            {
                return HttpNotFound("Termin nije pronadjen ili nije prosao.");
            }

            termin.OpisTerapije = opisTerapije;

            SaveTerapijaToFile(termin);

            return RedirectToAction("Index");
        }

        private void SaveTerapijaToFile(Termin termin)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Termini.txt");

            // Append mode, true ensures it doesn't overwrite the existing file content
            /* using (StreamWriter writer = new StreamWriter(filePath, true))
             {
                 string terminData = $"{termin.DanTermina},{termin.DatumZakazanogTermina},{termin.StatusTermina},{termin.Lekar.Id},{termin.OpisTerapije},{termin.Id},{(termin.Pacijent != null ? termin.Pacijent.Id.ToString() : "")}";

                 writer.WriteLine(terminData); // Dodaj novi termin u fajl
             }
            */
            Termini.ListaTermina[Termini.ListaTermina.IndexOf(termin)]=termin;
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (Termin t in Termini.ListaTermina)
                {
                    writer.WriteLine(t);
                }
            }
        }


        [HttpPost]
        public ActionResult Sortiranje(string parametarSortiranja, string redosledSortiranja)
        {
            var filteredTermini = Session["FiltredTermini"] as List<Termin>;
            if (filteredTermini == null)
            {
                return RedirectToAction("Index");
            }

            switch (parametarSortiranja)
            {
                case "Jmbg":
                    filteredTermini = redosledSortiranja == "asc" ?
                        filteredTermini.OrderBy(p => p.Pacijent?.Jmbg ?? string.Empty).ToList() :
                        filteredTermini.OrderByDescending(p => p.Pacijent?.Jmbg ?? string.Empty).ToList();
                    break;
                case "Ime":
                    filteredTermini = redosledSortiranja == "asc" ?
                        filteredTermini.OrderBy(p => p.Pacijent?.Ime ?? string.Empty).ToList() :
                        filteredTermini.OrderByDescending(p => p.Pacijent?.Ime ?? string.Empty).ToList();
                    break;
                case "Prezime":
                    filteredTermini = redosledSortiranja == "asc" ?
                        filteredTermini.OrderBy(p => p.Pacijent?.Prezime ?? string.Empty).ToList() :
                        filteredTermini.OrderByDescending(p => p.Pacijent?.Prezime ?? string.Empty).ToList();
                    break;
                case "DatumIVreme":
                    filteredTermini = redosledSortiranja == "asc" ?
                        filteredTermini.OrderBy(p => p.DatumZakazanogTermina).ToList() :
                        filteredTermini.OrderByDescending(p => p.DatumZakazanogTermina).ToList();
                    break;
                case "Status":
                    filteredTermini = redosledSortiranja == "asc" ?
                        filteredTermini.OrderBy(p => p.StatusTermina).ToList() :
                        filteredTermini.OrderByDescending(p => p.StatusTermina).ToList();
                    break;
                default:
                    break;
            }
            Lekar prijavljeniLekar = Lekari.LekariLista.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);
            Tuple<Lekar, List<Termin>> tuple = new Tuple<Lekar, List<Termin>>(prijavljeniLekar, filteredTermini);
            return View("Index", tuple);
        }

        public ActionResult ResetovanjeSortiranja()
        {
            var filteredTermini = Session["FiltredTermini"] as List<Termin>;
            if (filteredTermini == null)
            {
                return RedirectToAction("Index");
            }

            Lekar prijavljeniLekar = Lekari.LekariLista.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);
            Tuple<Lekar, List<Termin>> tuple = new Tuple<Lekar, List<Termin>>(prijavljeniLekar, filteredTermini);
            return View("Index", tuple);
        }

        [HttpPost]
        public ActionResult Filtriranje(string parametar, string vrednost)
        {
            var sviTermini = Session["SviTermini"] as List<Termin>;
            if (sviTermini == null)
            {
                return RedirectToAction("Index");
            }

            List<Termin> filteredTermini;
            switch (parametar)
            {
                case "Jmbg":
                    filteredTermini = sviTermini.Where(p => p.Pacijent?.Jmbg.Contains(vrednost) ?? false).ToList();
                    break;
                case "Ime":
                    filteredTermini = sviTermini.Where(p => p.Pacijent?.Ime.Contains(vrednost) ?? false).ToList();
                    break;
                case "Prezime":
                    filteredTermini = sviTermini.Where(p => p.Pacijent?.Prezime.Contains(vrednost) ?? false).ToList();
                    break;
                case "Status":
                    filteredTermini = sviTermini.Where(p => p.StatusTermina == IzStringUEnum(vrednost)).ToList();
                    break;
                case "DatumIVreme":
                    if (DateTime.TryParse(vrednost, out DateTime parsedDate))
                    {
                        filteredTermini = sviTermini.Where(p => p.DatumZakazanogTermina.Date == parsedDate.Date).ToList();
                    }
                    else
                    {
                        filteredTermini = new List<Termin>();
                    }
                    break;
                default:
                    filteredTermini = new List<Termin>();
                    break;
            }

            Session["FiltredTermini"] = filteredTermini;
            Lekar prijavljeniLekar = Lekari.LekariLista.FirstOrDefault(l => l.Id == ((PomocnaSesija)Session["auth"]).Id);
            Tuple<Lekar, List<Termin>> tuple = new Tuple<Lekar, List<Termin>>(prijavljeniLekar, filteredTermini);
            return View("Index", tuple);
        }

        private StatusTermina IzStringUEnum(string statusTermina)
        {
            if(statusTermina.Equals("Slobodan"))
            {
                return StatusTermina.Slobodan;
            }
            else
            {
                return StatusTermina.Zauzet;
            }
        }

    }
}