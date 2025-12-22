using Histo_Product.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Histo_Product.Controllers
{
    public class ProductController : Controller
    {
        private readonly Model1 db = new Model1();

        public ActionResult Dashboard()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "Log");

            var tests = db.Tests
                .Include("Board")
                .Include("Operateur")
                .Include("Machine")
  
                .Include("Product")
                .Include("Product.Ligne")
                .ToList();

            ViewBag.BoardList = db.Boards.ToList();
            ViewBag.OperateurList = db.Operateurs
            .Where(o => db.Profils.Any(l => l.Id_Operateur == o.Id && l.Role == "Operateur"))
            .ToList();
            ViewBag.LigneList = db.Lignes.ToList();
            ViewBag.MachineList = db.Machines.Where(m => m.EstActif).ToList();

            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.UntestedProducts = db.Products.Count(p => !p.Tests.Any());

            var operateurConnecte = GetOperateurConnecte();
            ViewBag.OperateurConnecte = operateurConnecte;

            ViewBag.TypesTestAutorises = new[] {
                "Test Initial", "Re-test Initial",
                "Contrôle Qualité", "Re-contrôle Qualité",
                "Test Client", "Re-test Client"
            };
            var tousProduits = db.Products.Include("Tests").ToList();
            var produitsRejetesCount = tousProduits.Count(p => EstProduitRejeteGlobal(p.Num_Serie));
            ViewBag.RejectedProductsCount = produitsRejetesCount;
            var produitsTerminesCount = tousProduits.Count(p =>p.Tests.Any(t => t.TypeTest == "Test Client" && t.Result == true));
            ViewBag.TerminedProductsCount = produitsTerminesCount;

            return View(tests);
        }

        public ActionResult UntestedProducts()
        {
            var untestedProducts = db.Products
                .Include("Ligne")
                .Where(p => !p.Tests.Any())
                .ToList();

            ViewBag.LigneList = db.Lignes.ToList();
            return View(untestedProducts);
        }

        public ActionResult ProduitsRejetes()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "Log");

            var tousProduits = db.Products
                .Include("Ligne")
                .Include("Tests")
                .Include("Tests.Board")
                .Include("Tests.Operateur")
                .Include("Tests.Machine")
                .ToList();

            var produitsRejetes = tousProduits.Where(p => EstProduitRejeteGlobal(p.Num_Serie)).ToList();

            ViewBag.TotalRejetes = produitsRejetes.Count;

            return View(produitsRejetes);
            
        }
        public ActionResult ProduitsTermines()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "Log");

            var tousProduits = db.Products
                .Include("Ligne")
                .Include("Tests")
                .Include("Tests.Board")
                .Include("Tests.Operateur")
                .Include("Tests.Machine")
                .ToList();

            var produitsTermines = tousProduits.Where(p =>
                p.Tests.Any(t => t.TypeTest == "Test Client" && t.Result == true)
            ).ToList();

            ViewBag.TotalTermines = produitsTermines.Count;
            return View(produitsTermines);
        }

        public JsonResult CheckTestHistory(string numSerie)
        {
            if (string.IsNullOrWhiteSpace(numSerie))
                return Json(null, JsonRequestBehavior.AllowGet);

            numSerie = numSerie.Trim().ToUpper();

            var produit = db.Products.FirstOrDefault(p => p.Num_Serie == numSerie);
            if (produit == null)
            {
                return Json(new
                {
                    produitExiste = false,
                    message = "Produit non trouvé. Créez d'abord le produit."
                }, JsonRequestBehavior.AllowGet);
            }

            var tests = db.Tests
                .Where(t => t.Num_Serie == numSerie)
                .OrderByDescending(t => t.DateDebut)
                .ToList();

            var testEnCours = tests.FirstOrDefault(t => t.Result == null);
            var testInitial = tests.FirstOrDefault(t => t.TypeTest == "Test Initial");
            var testReussi = tests.FirstOrDefault(t => t.Result == true);
            var dernierTest = tests.FirstOrDefault();

            var suggestion = GetSuggestionLogique(testInitial, tests, dernierTest, testEnCours);
            var statutProduit = GetStatutProduit(testInitial, tests, dernierTest, testEnCours);
            var suggestedBoardId = GetSuggestedBoardId(suggestion, testInitial, dernierTest);

            var result = new
            {
                produitExiste = true,
                hasTestInitial = testInitial != null,
                testInitialResult = testEnCours?.TypeTest == "Test Initial" ? null : testInitial?.Result,
                dernierTestType = dernierTest?.TypeTest,
                dernierTestResult = dernierTest?.Result,
                totalTests = tests.Count,
                suggestion = suggestion,
                statutProduit = statutProduit,
                idLigneProduit = produit.Ligne?.Id,
                nomLigneProduit = produit.Ligne?.Nom_Ligne,
                suggestedBoardId = suggestedBoardId,
                suggestedBoardName = suggestedBoardId.HasValue ?
                    db.Boards.Find(suggestedBoardId.Value)?.Code_AsteelFlash + " - " +
                    db.Boards.Find(suggestedBoardId.Value)?.Designation : null,
                autoDetected = suggestedBoardId.HasValue,
                hasReTestReussi = tests.Any(t => t.TypeTest == "Re-test Initial" && t.Result == true),
                canDoControleQualite = CanDoControleQualite(tests),
                testEnCours = testEnCours != null,
                testEnCoursId = testEnCours?.Id,
                testEnCoursType = testEnCours?.TypeTest,
                estRejete = EstProduitRejeteGlobal(numSerie),
                typeRejete = EstProduitRejeteGlobal(numSerie) ? GetTypeTestRejete(numSerie) : null
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: API pour historique complet des tests
        // GET: API pour historique complet des tests
        public JsonResult GetTestHistory(string numSerie)
        {
            if (string.IsNullOrWhiteSpace(numSerie))
                return Json(null, JsonRequestBehavior.AllowGet);

            numSerie = numSerie.Trim().ToUpper();

            var tests = db.Tests
                .Include(t => t.Operateur) // ✅ AJOUTÉ
                .Include(t => t.Machine)   // ✅ AJOUTÉ  
                .Include(t => t.Board)     // ✅ AJOUTÉ
                .Where(t => t.Num_Serie == numSerie)
                .OrderByDescending(t => t.DateDebut)
                .Select(t => new
                {
                    typeTest = t.TypeTest,
                    result = t.Result,
                    operateur = t.Operateur != null ? t.Operateur.Nom + " " + t.Operateur.Prénom : "Inconnu",
                    machine = t.Machine != null ? t.Machine.NomMachine : "—",
                    board = t.Board != null ? t.Board.Designation : "—"
                })
                .ToList();

            return Json(new { tests = tests }, JsonRequestBehavior.AllowGet);
        }

        // POST: Enregistrer un nouveau test
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(string numSerie = "", long idBoard = 0, long idOperateur = 0,
                                 string typeTest = "Test Initial", decimal idMachine = 0)
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "Log");

            try
            {
                // VALIDATION : Le produit doit exister AVANT de faire un test
                if (string.IsNullOrWhiteSpace(numSerie))
                {
                    TempData["Error"] = "Numéro de série obligatoire !";
                    return RedirectToAction("Dashboard");
                }

                numSerie = numSerie.Trim().ToUpper();

                // VÉRIFIER SI LE PRODUIT EXISTE
                var produit = db.Products.FirstOrDefault(p => p.Num_Serie == numSerie);
                if (produit == null)
                {
                    TempData["Error"] = $"Le produit {numSerie} n'existe pas. Créez d'abord le produit.";
                    return RedirectToAction("Dashboard");
                }

                // VÉRIFIER TEST EN COURS
                var testEnCours = db.Tests.FirstOrDefault(t => t.Num_Serie == numSerie && t.Result == null);
                if (testEnCours != null)
                {
                    TempData["Error"] = $"Impossible : Test #{testEnCours.Id} ({testEnCours.TypeTest}) est déjà en cours pour {numSerie}. Attendez sa complétion.";
                    return RedirectToAction("Dashboard");
                }

                // VALIDATIONS OBLIGATOIRES
                if (idBoard == 0 || idOperateur == 0 || idMachine == 0)
                {
                    TempData["Error"] = "Board, Opérateur et Machine sont obligatoires !";
                    return RedirectToAction("Dashboard");
                }

                // VÉRIFICATION AUTORISATION OPÉRATEUR
                var operateur = db.Operateurs.Find(idOperateur);
                if (operateur == null || !EstTypeTestAutorise(typeTest, operateur.Fonction))
                {
                    TempData["Error"] = $"Opérateur non autorisé pour {typeTest}";
                    return RedirectToAction("Dashboard");
                }

                // RÉCUPÉRER LA MACHINE POUR LE SOFTWARE
                var machine = db.Machines.Find(idMachine);
                if (machine == null)
                {
                    TempData["Error"] = "Machine non trouvée";
                    return RedirectToAction("Dashboard");
                }

                // VALIDATION DES RÈGLES MÉTIER POUR LE TYPE DE TEST
                if (!ValiderReglesMetier(numSerie, typeTest))
                {
                    return RedirectToAction("Dashboard");
                }

                // CRÉATION DU TEST
                var test = new Test
                {
                    Num_Serie = numSerie,
                    Id_Board = idBoard,
                    Id_Operateur = idOperateur,
                    Id_Machine = idMachine,
                    TestSoftwareVersion = machine.SoftwareVersion,
                     
                    DateDebut = DateTime.Now,
                    DateFin = null,
                    TypeTest = typeTest,
                    Result = null,
                    StatutTest = "En cours",
                    Id_Product = produit.Id
                };

                db.Tests.Add(test);
                db.SaveChanges();

                TempData["Success"] = $"Test #{test.Id} créé avec succès → {numSerie} ({typeTest}) - Machine: {machine.NomMachine}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur création test : " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }


        private Operateur GetOperateurConnecte()
        {
            if (Session["userId"] != null)
            {
                int userId = (int)Session["userId"];
                var login = db.Profils.Find(userId);
                if (login == null) return null;

                if (login.Id_Operateur.HasValue)
                {
                    return db.Operateurs.Find(login.Id_Operateur.Value);
                }

                return db.Operateurs.FirstOrDefault(o => o.Matricule == login.Username);
            }
            return null;
        }

        private string[] GetTypesTestAutorises(string fonction)
        {
            switch (fonction)
            {
                case "Technicien":
                    return new[] { "Test Initial", "Re-test Initial" };
                case "Ingénieur":
                    return new[] { "Contrôle Qualité", "Re-contrôle Qualité" };
                case "Superviseur":
                    return new[] { "Test Initial", "Re-test Initial", "Contrôle Qualité", "Re-contrôle Qualité", "Test Client", "Re-test Client" };
                default:
                    return new[] { "Test Initial" };
            }
        }

        private bool EstTypeTestAutorise(string typeTest, string fonction)
        {
            var typesAutorises = GetTypesTestAutorises(fonction);
            return typesAutorises.Contains(typeTest);
        }

        private bool ValiderReglesMetier(string numSerie, string typeTest)
        {
            var testsExistants = db.Tests.Where(t => t.Num_Serie == numSerie).ToList();

            if (EstProduitRejeteGlobal(numSerie))
            {
                var typeRejete = GetTypeTestRejete(numSerie);
                TempData["Error"] = $"Produit rejeté définitivement : 2 échecs sur {typeRejete}";
                return false;
            }

            if (typeTest == "Re-test Initial")
            {
                var testInitial = testsExistants.FirstOrDefault(t => t.TypeTest == "Test Initial");
                if (testInitial == null || testInitial.Result == true)
                {
                    TempData["Error"] = "Re-test Initial impossible : Test Initial manquant ou réussi";
                    return false;
                }

                // Vérifier qu'on n'a pas déjà 1 échec Re-test
                var echecsReTest = testsExistants.Count(t =>
                    t.TypeTest == "Re-test Initial" && t.Result == false);
                if (echecsReTest >= 1)
                {
                    TempData["Error"] = "Re-test Initial impossible : Déjà 1 échec sur Re-test Initial";
                    return false;
                }
            }
            else if (typeTest == "Re-contrôle Qualité")
            {
                var controleQualite = testsExistants.FirstOrDefault(t => t.TypeTest == "Contrôle Qualité");
                if (controleQualite == null || controleQualite.Result == true)
                {
                    TempData["Error"] = "Re-contrôle Qualité impossible : Contrôle Qualité manquant ou réussi";
                    return false;
                }
            }
            else if (typeTest == "Re-test Client")
            {
                var testClient = testsExistants.FirstOrDefault(t => t.TypeTest == "Test Client");
                if (testClient == null || testClient.Result == true)
                {
                    TempData["Error"] = "Re-test Client impossible : Test Client manquant ou réussi";
                    return false;
                }
            }
            else if (typeTest == "Contrôle Qualité")
            {
                var testReussi = testsExistants.FirstOrDefault(t =>
                    (t.TypeTest == "Test Initial" || t.TypeTest == "Re-test Initial") && t.Result == true);
                if (testReussi == null)
                {
                    TempData["Error"] = "Contrôle Qualité impossible : Aucun test initial réussi";
                    return false;
                }
            }
            else if (typeTest == "Test Client")
            {
                var controleReussi = testsExistants.FirstOrDefault(t =>
                    (t.TypeTest == "Contrôle Qualité" || t.TypeTest == "Re-contrôle Qualité") && t.Result == true);
                if (controleReussi == null)
                {
                    TempData["Error"] = "Test Client impossible : Aucun contrôle qualité réussi";
                    return false;
                }
            }

            var typesDeBase = new[] { "Test Initial", "Contrôle Qualité", "Test Client" };
            if (typesDeBase.Contains(typeTest))
            {
                var testExistant = testsExistants.FirstOrDefault(t => t.TypeTest == typeTest);
                if (testExistant != null)
                {
                    TempData["Error"] = $"Un {typeTest} existe déjà pour ce produit";
                    return false;
                }
            }

            return true;
        }

        private bool EstProduitRejeteGlobal(string numSerie)
        {
            var testsExistants = db.Tests.Where(t => t.Num_Serie == numSerie).ToList();

            // ✅ CORRECTION : GROUPER LES TYPES SIMILAIRES
            var echecsTestInitial = testsExistants.Count(t =>
                (t.TypeTest == "Test Initial" || t.TypeTest == "Re-test Initial")
                && t.Result == false);

            var echecsControleQualite = testsExistants.Count(t =>
                (t.TypeTest == "Contrôle Qualité" || t.TypeTest == "Re-contrôle Qualité")
                && t.Result == false);

            var echecsTestClient = testsExistants.Count(t =>
                (t.TypeTest == "Test Client" || t.TypeTest == "Re-test Client")
                && t.Result == false);

            // ✅ REJET si 2 échecs sur un groupe
            return echecsTestInitial >= 2 || echecsControleQualite >= 2 || echecsTestClient >= 2;
        }

        private string GetTypeTestRejete(string numSerie)
        {
            var testsExistants = db.Tests.Where(t => t.Num_Serie == numSerie).ToList();

            var echecsTestInitial = testsExistants.Count(t =>
                (t.TypeTest == "Test Initial" || t.TypeTest == "Re-test Initial") && t.Result == false);
            if (echecsTestInitial >= 2) return "Test Initial";

            var echecsControleQualite = testsExistants.Count(t =>
                (t.TypeTest == "Contrôle Qualité" || t.TypeTest == "Re-contrôle Qualité") && t.Result == false);
            if (echecsControleQualite >= 2) return "Contrôle Qualité";

            var echecsTestClient = testsExistants.Count(t =>
                (t.TypeTest == "Test Client" || t.TypeTest == "Re-test Client") && t.Result == false);
            if (echecsTestClient >= 2) return "Test Client";

            return "Inconnu";
        }

        private bool CanDoControleQualite(System.Collections.Generic.List<Test> tests)
        {
            var testInitialReussi = tests.FirstOrDefault(t => t.TypeTest == "Test Initial" && t.Result == true);
            var reTestReussi = tests.FirstOrDefault(t => t.TypeTest == "Re-test Initial" && t.Result == true);
            return (testInitialReussi != null) || (reTestReussi != null);
        }

        private decimal? GetSuggestedBoardId(string suggestion, Test testInitial, Test dernierTest)
        {
            switch (suggestion)
            {
                case "Test Initial": return 1;
                case "Re-test Initial": return 1;
                case "Contrôle Qualité": return 2;
                case "Re-contrôle Qualité": return 2;
                case "Test Client": return 3;
                case "Re-test Client": return 3;
                case "PRODUIT REJETÉ - Aucun test possible": return null;
                default: return 1;
            }
        }

        private string GetSuggestionLogique(Test testInitial, System.Collections.Generic.List<Test> tousTests, Test dernierTest, Test testEnCours)
        {
            if (testEnCours != null)
            {
                return "Attendre résultat test en cours";
            }

            // ✅ VÉRIFIER REJET GLOBAL EN PREMIER !
            if (testInitial != null && EstProduitRejeteGlobal(testInitial.Num_Serie))
            {
                return "PRODUIT REJETÉ - Aucun test possible";
            }

            if (testInitial == null) return "Test Initial";

            // ✅ CORRECTION : ANALYSER LE DERNIER TEST EN PREMIER
            if (dernierTest != null)
            {
                // Si le dernier test a ÉCHOUÉ → suggérer le RE-TEST correspondant
                if (dernierTest.Result == false)
                {
                    switch (dernierTest.TypeTest)
                    {
                        case "Test Initial":
                            // Vérifier si un re-test initial existe déjà
                            var aDejaReTestInitial = tousTests.Any(t => t.TypeTest == "Re-test Initial");
                            return aDejaReTestInitial ? "PRODUIT REJETÉ - Aucun test possible" : "Re-test Initial";

                        case "Contrôle Qualité":
                            // Vérifier si un re-contrôle qualité existe déjà  
                            var aDejaReControleQualite = tousTests.Any(t => t.TypeTest == "Re-contrôle Qualité");
                            return aDejaReControleQualite ? "PRODUIT REJETÉ - Aucun test possible" : "Re-contrôle Qualité";

                        case "Test Client":
                            // Vérifier si un re-test client existe déjà
                            var aDejaReTestClient = tousTests.Any(t => t.TypeTest == "Re-test Client");
                            return aDejaReTestClient ? "PRODUIT REJETÉ - Aucun test possible" : "Re-test Client";
                    }
                }

                // Si le dernier test a RÉUSSI → passer à l'étape suivante
                if (dernierTest.Result == true)
                {
                    switch (dernierTest.TypeTest)
                    {
                        case "Test Initial":
                        case "Re-test Initial":
                            return "Contrôle Qualité";

                        case "Contrôle Qualité":
                        case "Re-contrôle Qualité":
                            return "Test Client";

                        case "Test Client":
                            return "TERMINÉ - Tous tests validés";
                    }
                }
            }

            // ✅ Logique de fallback (si pas de dernier test ou résultat null)
            bool peutFaireControleQualite = CanDoControleQualite(tousTests);

            if (testInitial.Result == false)
            {
                bool aReTestReussi = tousTests.Any(t => t.TypeTest == "Re-test Initial" && t.Result == true);
                return aReTestReussi ? "Contrôle Qualité" : "Re-test Initial";
            }

            if (testInitial.Result == true)
            {
                bool aControleQualite = tousTests.Any(t => t.TypeTest == "Contrôle Qualité");
                return aControleQualite ? "Test Client" : "Contrôle Qualité";
            }

            return "Test Initial";
        }

        private string GetStatutProduit(Test testInitial, System.Collections.Generic.List<Test> tousTests, Test dernierTest, Test testEnCours)
        {
            if (testEnCours != null)
            {
                return $"Test {testEnCours.TypeTest} en cours - #{testEnCours.Id}";
            }

            // Vérifier rejet global
            if (testInitial != null && EstProduitRejeteGlobal(testInitial.Num_Serie))
            {
                var typeRejete = GetTypeTestRejete(testInitial.Num_Serie);
                return $"REJETÉ DÉFINITIF - 2 échecs sur {typeRejete}";
            }
             
            if (testInitial == null) return "Nouveau - Prêt pour Test Initial";

            var reTestReussi = tousTests.FirstOrDefault(t => t.TypeTest == "Re-test Initial" && t.Result == true);
            var controleQualite = tousTests.FirstOrDefault(t => t.TypeTest == "Contrôle Qualité");
            var reControleQualite = tousTests.FirstOrDefault(t => t.TypeTest == "Re-contrôle Qualité");
            var testClient = tousTests.FirstOrDefault(t => t.TypeTest == "Test Client");
            var reTestClient = tousTests.FirstOrDefault(t => t.TypeTest == "Re-test Client");

            if (testClient?.Result == true) return "TERMINÉ - Tous les tests validés";
            if (reTestClient?.Result == true) return "TERMINÉ - Test Client réussi après re-test";
            if (controleQualite?.Result == true) return "Contrôle Qualité réussi - Prêt pour Test Client";
            if (reControleQualite?.Result == true) return "Re-contrôle Qualité réussi - Prêt pour Test Client";
            if (reTestReussi != null) return "Retester avec succès - Prêt pour Contrôle Qualité";
            if (testInitial.Result == false) return "En échec - Retester nécessaire";
            if (testInitial.Result == true) return "Test Initial réussi - Prêt pour Contrôle Qualité";

            return "En cours de test";
        }
        // GET: Recherche
        public ActionResult Recherche()
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "Log");
            return View();
        }

        // POST: Recherche
        // POST: Recherche
        [HttpPost]
        public ActionResult Recherche(string numSerie, DateTime? startDate, DateTime? endDate,
                            string resultat, string typeTest, string actionType)
        {
            if (Session["user"] == null)
                return RedirectToAction("Login", "Log");

            try
            {
                var query = db.Tests
                    .Include("Board")
                    .Include("Operateur")
                    .Include("Machine")
                    .AsQueryable();

                bool hasFilters = false; // 🔧 NOUVEAU : vérifier si des filtres sont appliqués

                // Filtre Numéro de Série
                if (!string.IsNullOrEmpty(numSerie))
                {
                    numSerie = numSerie.Trim().ToUpper();
                    query = query.Where(t => t.Num_Serie.ToUpper().Contains(numSerie));
                    hasFilters = true; // 🔧 Un filtre est appliqué
                }

                // Filtres de date
                if (startDate.HasValue)
                {
                    var dateDebut = startDate.Value.Date;
                    query = query.Where(t => t.DateDebut.HasValue &&
                                           DbFunctions.TruncateTime(t.DateDebut.Value) >= dateDebut);
                    hasFilters = true; // 🔧 Un filtre est appliqué
                }

                if (endDate.HasValue)
                {
                    var dateFin = endDate.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(t => t.DateDebut.HasValue &&
                                           t.DateDebut.Value <= dateFin);
                    hasFilters = true; // 🔧 Un filtre est appliqué
                }

                // Filtre Résultat
                if (!string.IsNullOrEmpty(resultat))
                {
                    bool resultBool = resultat == "True";
                    query = query.Where(t => t.Result == resultBool);
                    hasFilters = true; // 🔧 Un filtre est appliqué
                }

                // Filtre Type Test
                if (!string.IsNullOrEmpty(typeTest))
                {
                    query = query.Where(t => t.TypeTest.Contains(typeTest));
                    hasFilters = true; // 🔧 Un filtre est appliqué
                }

                // 🔧 CORRECTION : Si AUCUN filtre n'est appliqué, retourner une vue vide
                if (!hasFilters)
                {
                    ViewBag.Message = "Veuillez spécifier au moins un critère de recherche.";
                    return View(new List<Test>()); // Retourne une liste vide
                }

                var results = query.OrderByDescending(t => t.DateDebut).ToList();

                if (actionType == "ExporterExcel")
                {
                    return ExportToExcel(results);
                }

                ViewBag.ResultsCount = results.Count;
                return View(results);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Erreur lors de la recherche : " + ex.Message;
                return View();
            }
        }

        private ActionResult ExportToExcel(List<Test> tests)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Recherche Tests");

                    string[] headers = { "ID Test", "Numéro Série", "Date Début", "Type Test", "Machine", "Software", "Board", "Opérateur", "Résultat" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    }

                    int row = 2;
                    foreach (var test in tests)
                    {
                        worksheet.Cells[row, 1].Value = test.Id;
                        worksheet.Cells[row, 2].Value = test.Num_Serie;
                        worksheet.Cells[row, 3].Value = test.DateDebut?.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cells[row, 4].Value = test.TypeTest;
                        worksheet.Cells[row, 5].Value = test.Machine?.NomMachine ?? "—";
                        worksheet.Cells[row, 6].Value = test.TestSoftwareVersion ?? "—";
                        worksheet.Cells[row, 7].Value = test.Board?.Designation ?? "—";
                        worksheet.Cells[row, 8].Value = test.Operateur != null ?
                            $"{test.Operateur.Nom} {test.Operateur.Prénom}" : "Inconnu";

                        var resultCell = worksheet.Cells[row, 9];
                        resultCell.Value = test.Result == true ? "RÉUSSI" :
                                         test.Result == false ? "ÉCHEC" : "EN COURS";

                        if (test.Result == true)
                            resultCell.Style.Font.Color.SetColor(System.Drawing.Color.Green);
                        else if (test.Result == false)
                            resultCell.Style.Font.Color.SetColor(System.Drawing.Color.Red);

                        row++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var fileName = $"Recherche_Tests_{DateTime.Now:yyyyMMddHHmm}.xlsx";
                    return File(package.GetAsByteArray(),
                               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                               fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur export Excel : " + ex.Message;
                return RedirectToAction("Recherche");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}