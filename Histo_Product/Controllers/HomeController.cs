using System;
using System.Linq;
using System.Web.Mvc;
using Histo_Product.Models;

public class HomeController : Controller
{
    private Model1 db = new Model1();  // Comme vos autres controllers

    public ActionResult Recherche()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Recherche(string numSerie, DateTime? startDate, DateTime? endDate,
                                string resultat, string typeTest, string actionType)
    {
        try
        {
            // Validation des critères
            if (string.IsNullOrEmpty(numSerie) && !startDate.HasValue &&
                !endDate.HasValue && string.IsNullOrEmpty(resultat) &&
                string.IsNullOrEmpty(typeTest))
            {
                ViewBag.Message = "Veuillez remplir au moins un critère de recherche.";
                return View();
            }

            // Construction de la requête
            var query = db.Tests.AsQueryable();

            if (!string.IsNullOrEmpty(numSerie))
                query = query.Where(t => t.Num_Serie.Contains(numSerie));

            if (startDate.HasValue)
                query = query.Where(t => t.DateDebut >= startDate);

            if (endDate.HasValue)
                query = query.Where(t => t.DateDebut <= endDate.Value.AddDays(1));

            if (!string.IsNullOrEmpty(resultat))
            {
                bool resultBool = resultat == "True";
                query = query.Where(t => t.Result == resultBool);
            }

            if (!string.IsNullOrEmpty(typeTest))
                query = query.Where(t => t.TypeTest.Contains(typeTest));

            var results = query.OrderByDescending(t => t.DateDebut).ToList();

            // Export Excel (à implémenter)
            if (actionType == "ExporterExcel")
            {
                // Votre logique d'export Excel ici
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

    private ActionResult ExportToExcel(object data)
    {
        // Implémentation de l'export Excel
        return new EmptyResult();
    }
}