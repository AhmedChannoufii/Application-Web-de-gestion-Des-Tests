using System;
using System.Linq;
using System.Web.Mvc;
using Histo_Product.Models;

namespace Histo_Product.Controllers
{
    public class OperateurController : Controller
    {
        private readonly Model1 db = new Model1();

        public ActionResult Dashboard()
        {
            if (Session["user"] == null || Session["role"]?.ToString() != "Operateur")
                return RedirectToAction("Login", "Log");

            var userId = (int)Session["userId"];
            var login = db.Profils.Find(userId); // ← SEUL CHANGEMENT ICI

            if (login?.Id_Operateur == null)
            {
                TempData["Error"] = "Aucun opérateur lié à ce compte";
                return RedirectToAction("Login", "Log");
            }

            var operateur = db.Operateurs
                .FirstOrDefault(o => o.Id == login.Id_Operateur.Value);

            if (operateur == null)
            {
                TempData["Error"] = "Opérateur non trouvé";
                return RedirectToAction("Login", "Log");
            }

            var testsAssignes = db.Tests
                .Include("Board")
                .Include("Machine")
                .Include("Product")
                .Include("Operateur")
                .Where(t => t.Id_Operateur == operateur.Id)
                .OrderByDescending(t => t.DateDebut)
                .ToList();

            ViewBag.OperateurConnecte = operateur;
            ViewBag.HideNavbar = true;
            return View(testsAssignes);
        }

        [HttpPost]
        public JsonResult SelectionnerResultat(int testId, bool? resultat)
        {
            try
            {
                var sql = @"
                    UPDATE Test 
                    SET ResultatTemporaire = @result, 
                        DateSelection = GETDATE(),
                        StatutTest = 'En attente'
                    WHERE Id = @testId";

                var parameters = new object[]
                {
                    new System.Data.SqlClient.SqlParameter("@result", resultat ?? (object)DBNull.Value),
                    new System.Data.SqlClient.SqlParameter("@testId", testId)
                };

                db.Database.ExecuteSqlCommand(sql, parameters);

                return Json(new
                {
                    success = true,
                    message = "Résultat sélectionné"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Erreur: " + ex.Message
                });
            }
        }

        [HttpPost]
        public JsonResult ConfirmerResultat(int testId)
        {
            try
            {
                var sql = @"
                    UPDATE Test 
                    SET Result = ResultatTemporaire,
                        DateFin = GETDATE(),
                        StatutTest = 'Terminé',
                        DateFinReel = GETDATE(),
                        ResultatTemporaire = NULL,
                        DateSelection = NULL
                    WHERE Id = @testId 
                    AND ResultatTemporaire IS NOT NULL";

                var parameters = new object[]
                {
                    new System.Data.SqlClient.SqlParameter("@testId", testId)
                };

                var rowsAffected = db.Database.ExecuteSqlCommand(sql, parameters);

                if (rowsAffected > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Résultat confirmé définitivement"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Aucune sélection temporaire trouvée"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Erreur: " + ex.Message
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}