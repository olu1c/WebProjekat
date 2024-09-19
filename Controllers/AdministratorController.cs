using Projekat.Helperi;
using Projekat.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Projekat.Controllers
{
    public class AdministratorController : Controller
    {
        
        public ActionResult Index()
        {
            if (Session["auth"] != null)
            {
                if (((PomocnaSesija)Session["auth"]).Podela == "A")
                {
                    List<Pacijent> listaPacijenata = Pacijenti.PacijentiList;
                    Session["FilteredPacijenti"] = listaPacijenata;
                    return View(listaPacijenata);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult KreirajPacijenta(string korisnickoIme, string jmbg, string ime, string prezime, string email,string sifra, DateTime datumRodjenja)
        {
            if (Session["auth"] == null || ((PomocnaSesija)Session["auth"]).Podela != "A")
            {
                return RedirectToAction("Index", "Home");
            }


            if (Pacijenti.PacijentiList.Any(p => p.KorisnickoIme == korisnickoIme && !p.IsDeleted))
            {
                ViewBag.ErrorMessage = "Korisnicko ime vec postoji.";
                return View();
            }
            if (Pacijenti.PacijentiList.Any(p => p.Jmbg == jmbg && !p.IsDeleted))
            {
                ViewBag.ErrorMessage = "JMBG vec postoji.";
                return View();
            }
            if (jmbg.Length != 13)
            {
                ViewBag.ErrorMessage = "JMBG nije validne duzine(mora biti 13 karaktera).";
                return View();
            }
            if (Pacijenti.PacijentiList.Any(p => p.ElektronskaPosta == email && !p.IsDeleted))
            {
                ViewBag.ErrorMessage = "Email vec postoji.";
                return View();
            }

            string emailPattern = @"^[\w\.-]+@gmail\.com$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                ViewBag.ErrorMessage = "Email mora biti u formatu nesto@gmail.com.";
                return View();
            }

            string jmbgPattern = @"^\d{13}$";
            if (!Regex.IsMatch(jmbg, jmbgPattern))
            {
                ViewBag.ErrorMessage = "JMBG mora sadrzati samo brojeve i imati tacno 13 cifara.";
                return View();
            }

            // Kreiranje novog pacijenta
            Pacijent noviPacijent = new Pacijent
            {
                KorisnickoIme = korisnickoIme,
                Jmbg = jmbg,
                Ime = ime,
                Prezime = prezime,
                ElektronskaPosta = email,
                DatumRodjenja = new DateTime(datumRodjenja.Year, datumRodjenja.Month, datumRodjenja.Day, 14, 30, 0),
                Sifra = sifra, 
                ZakazaniTermini = new List<Termin>(),
                Id = Pacijenti.PacijentiList.Count + 1 
            };

            Pacijenti.PacijentiList.Add(noviPacijent);

            SaveToFile(noviPacijent);

            return RedirectToAction("index", "Administrator");
        }

        private void SaveToFile(Pacijent pacijent)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Pacijenti.txt");

            string pacijentData = $"\n{pacijent.KorisnickoIme},{pacijent.Jmbg},{pacijent.Sifra},{pacijent.Ime},{pacijent.Prezime},{pacijent.DatumRodjenja},{pacijent.ElektronskaPosta},{pacijent.Id},{pacijent.IsDeleted}";
            
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(pacijentData);
            }

        }

        public ActionResult ListaPacijenata()
        {
            List<Pacijent> listaPacijenata = Pacijenti.PacijentiList; 
            return View(listaPacijenata); 
        }

        public ActionResult IzmeniPacijenta(int id)
        {
            Pacijent pacijent = Pacijenti.PacijentiList.FirstOrDefault(p => p.Id == id);
            if (pacijent == null)
            {
                return HttpNotFound(); 
            }

            return View(pacijent); 
        }

        [HttpPost]
        public ActionResult SacuvajIzmenePacijenta(Pacijent model)
        {
            if (Session["auth"] == null || ((PomocnaSesija)Session["auth"]).Podela != "A")
            {
                return RedirectToAction("Index", "Home");
            }

            if (Pacijenti.PacijentiList.Any(p => p.Id != model.Id && p.ElektronskaPosta == model.ElektronskaPosta))
            {
                ViewBag.ErrorMessage = "Email adresa vec postoji.";
                return View("IzmeniPacijenta", model); 
            }

            Pacijent postojeciPacijent = Pacijenti.PacijentiList.FirstOrDefault(p => p.Id == model.Id);
            if (postojeciPacijent != null)
            {
                postojeciPacijent.Ime = model.Ime;
                postojeciPacijent.Prezime = model.Prezime;
                postojeciPacijent.DatumRodjenja = model.DatumRodjenja;
                postojeciPacijent.ElektronskaPosta = model.ElektronskaPosta;

                AzurirajPacijentaUFajlu(Pacijenti.PacijentiList);

                return RedirectToAction("Index"); 
            }

            return HttpNotFound();
        }

        private void AzurirajPacijentaUFajlu(List<Pacijent> pacijenti)
        {
            string filePath = Server.MapPath("~/App_Data/Pacijenti.txt");

            List<string> lines = new List<string>();

            foreach (Pacijent pacijent in pacijenti)
            {
                string line = $"{pacijent.KorisnickoIme},{pacijent.Jmbg},{pacijent.Sifra},{pacijent.Ime},{pacijent.Prezime},{pacijent.DatumRodjenja.ToString("M/d/yyyy h:mm:ss tt")},{pacijent.ElektronskaPosta},{pacijent.Id},{pacijent.IsDeleted}";
                lines.Add(line);
            }

            System.IO.File.WriteAllLines(filePath, lines);
        }

        private void LogicalDeleteInFile(Pacijent pacijent)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Pacijenti.txt");
            List<string> linije = new List<string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    linije.Add(line);
                }
            }

            for (int i = 0; i < linije.Count; i++)
            {
                string[] podaci = linije[i].Split(',');
                int id;
                if (podaci.Length >= 8 && int.TryParse(podaci[7], out id) && id == pacijent.Id)
                {
                    string updatedLine = $"{pacijent.KorisnickoIme},{pacijent.Jmbg},{pacijent.Sifra},{pacijent.Ime},{pacijent.Prezime},{pacijent.DatumRodjenja},{pacijent.ElektronskaPosta},{pacijent.Id},True";
                    linije[i] = updatedLine;
                    break;
                }
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (string line in linije)
                {
                    writer.WriteLine(line);
                }
            }
        }

        public ActionResult ObrisiPacijenta(int id)
        {
            if (Session["auth"] == null || ((PomocnaSesija)Session["auth"]).Podela != "A")
            {
                return RedirectToAction("Index", "Home");
            }

            Pacijent pacijentZaBrisanje = Pacijenti.PacijentiList.FirstOrDefault(p => p.Id == id);
            if (pacijentZaBrisanje != null)
            {
                LogicalDeleteInFile(pacijentZaBrisanje);

                pacijentZaBrisanje.IsDeleted = true;

                return RedirectToAction("Index"); 
            }

            return HttpNotFound(); 
        }

        [HttpPost]
        public ActionResult Sortiranje(string parametarSortiranja, string redosledSortiranja)
        {
            //var pacijenti = Pacijenti.PacijentiList.Where(p => !p.IsDeleted).ToList();

            var filteredPacijenti = Session["FilteredPacijenti"] as List<Pacijent>;
            if (filteredPacijenti == null)
            {
                return RedirectToAction("Index"); 
            }

            switch (parametarSortiranja)
            {
                case "KorisnickoIme":
                    filteredPacijenti = redosledSortiranja == "asc" ?
                                 filteredPacijenti.OrderBy(p => p.KorisnickoIme).ToList() :
                                 filteredPacijenti.OrderByDescending(p => p.KorisnickoIme).ToList();
                    break;
                case "Jmbg":
                    filteredPacijenti = redosledSortiranja == "asc" ?
                                 filteredPacijenti.OrderBy(p => p.Jmbg).ToList() :
                                 filteredPacijenti.OrderByDescending(p => p.Jmbg).ToList();
                    break;
                case "Ime":
                    filteredPacijenti = redosledSortiranja == "asc" ?
                                 filteredPacijenti.OrderBy(p => p.Ime).ToList() :
                                 filteredPacijenti.OrderByDescending(p => p.Ime).ToList();
                    break;
                case "Prezime":
                    filteredPacijenti = redosledSortiranja == "asc" ?
                                 filteredPacijenti.OrderBy(p => p.Prezime).ToList() :
                                 filteredPacijenti.OrderByDescending(p => p.Prezime).ToList();
                    break;
                case "DatumRodjenja":
                    filteredPacijenti = redosledSortiranja == "asc" ?
                                 filteredPacijenti.OrderBy(p => p.DatumRodjenja).ToList() :
                                 filteredPacijenti.OrderByDescending(p => p.DatumRodjenja).ToList();
                    break;
                case "Email":
                    filteredPacijenti = redosledSortiranja == "asc" ?
                                 filteredPacijenti.OrderBy(p => p.ElektronskaPosta).ToList() :
                                 filteredPacijenti.OrderByDescending(p => p.ElektronskaPosta).ToList();
                    break;
                default:
                    break;
            }

            return View("Index", filteredPacijenti);
        }

        public ActionResult ResetovanjeSortiranja()
        {
            var filteredPacijenti = Session["FilteredPacijenti"] as List<Pacijent>;
            if (filteredPacijenti == null)
            {
                return RedirectToAction("Index");
            }

            return View("Index", filteredPacijenti);
        }

        [HttpPost]
        public ActionResult Filtriranje(string parametar, string vrednost)
        {
            var pacijenti = Pacijenti.PacijentiList.Where(p => !p.IsDeleted).ToList();

            if (!string.IsNullOrEmpty(vrednost))
            {
                switch (parametar)
                {
                    case "Ime":
                        pacijenti = pacijenti.Where(p => p.Ime.Contains(vrednost)).ToList();
                        break;
                    case "Prezime":
                        pacijenti = pacijenti.Where(p => p.Prezime.Contains(vrednost)).ToList();
                        break;
                    case "Jmbg":
                        pacijenti = pacijenti.Where(p => p.Jmbg.Contains(vrednost)).ToList();
                        break;
                    case "DatumRodjenja":
                        if (DateTime.TryParse(vrednost, out DateTime datum))
                        {
                            pacijenti = pacijenti.Where(p => p.DatumRodjenja.Date == datum.Date).ToList();
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Neispravan format datuma.";
                        }
                        break;
                    case "Email":
                        pacijenti = pacijenti.Where(p => p.ElektronskaPosta.Contains(vrednost)).ToList();
                        break;
                    default:
                        ViewBag.ErrorMessage = "Nepoznat parametar za filtriranje.";
                        break;
                }
            }

            Session["FilteredPacijenti"] = pacijenti;
            return View("Index", pacijenti);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

    }


}


