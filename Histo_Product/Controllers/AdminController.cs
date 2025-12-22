using Histo_Product.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Histo_Product.Controllers
{
    public class AdminController : Controller
    {
        private Model1 db = new Model1();

        private bool IsAdmin()
        {
            return Session["role"]?.ToString() == "Administrateur";
        }

        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Log");
            return View();
        }

        public ActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Log");
            ViewBag.HideNavbar = true;

            ViewBag.OperateurCount = db.Profils.Count(u => u.Role == "Operateur");
            ViewBag.UtilisateurCount = db.Profils.Count(u => u.Role == "Utilisateur");

            return View();
        }

        public ActionResult UserManagement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Log");

            var users = db.Profils.ToList();
            return View(users);
        }

        [HttpPost]
        public JsonResult AddUser(string username, string password, string role, string email = null)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Accès non autorisé" });

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return Json(new { success = false, message = "Nom d'utilisateur et mot de passe requis" });

                if (role == "Administrateur")
                    return Json(new { success = false, message = "Impossible de créer un compte Administrateur" });

                var existingUser = db.Profils.FirstOrDefault(u => u.Username == username);
                if (existingUser != null)
                    return Json(new { success = false, message = "Cet utilisateur existe déjà" });

        
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var newUser = new Profil
                {
                    Username = username,
                    Password = hashedPassword,
                    Role = role,
                    Email = email, 
                    CreateDate = DateTime.Now
                };

                db.Profils.Add(newUser);
                db.SaveChanges();

                return Json(new { success = true, message = "Utilisateur créé avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ResetUserPassword(int userId, string newPassword)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Accès non autorisé" });

                var user = db.Profils.Find(userId);
                if (user == null)
                    return Json(new { success = false, message = "Utilisateur non trouvé" });

                if (user.Username == Session["user"]?.ToString())
                    return Json(new { success = false, message = "Vous ne pouvez pas modifier votre propre mot de passe" });

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                db.SaveChanges();

                return Json(new { success = true, message = "Mot de passe modifié avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }
        public ActionResult GestionOperateurs()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Log");

            // Récupérer les opérateurs avec/sans compte
            var operateursAvecCompte = db.Operateurs
                .Where(o => db.Profils.Any(l => l.Id_Operateur == o.Id))
                .ToList();

            var operateursSansCompte = db.Operateurs
                .Where(o => !db.Profils.Any(l => l.Id_Operateur == o.Id))
                .ToList();

            ViewBag.OperateursAvecCompte = operateursAvecCompte;
            ViewBag.OperateursSansCompte = operateursSansCompte;
            ViewBag.CountAvecCompte = operateursAvecCompte.Count;
            ViewBag.CountSansCompte = operateursSansCompte.Count;

            return View();
        }

        [HttpPost]
        public JsonResult CreateOperateurAccount(decimal operateurId, string password)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Accès non autorisé" });

                var operateur = db.Operateurs.Find(operateurId);
                if (operateur == null)
                    return Json(new { success = false, message = "Opérateur non trouvé" });

                // Vérifier si un compte existe déjà
                var existingAccount = db.Profils.FirstOrDefault(l => l.Id_Operateur == operateurId);
                if (existingAccount != null)
                    return Json(new { success = false, message = "Cet opérateur a déjà un compte" });

                // Créer le compte
                var newUser = new Profil
                {
                    Username = operateur.Matricule,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "Operateur",
                    Email = operateur.Email,
                    CreateDate = DateTime.Now,
                    Id_Operateur = operateur.Id
                };

                db.Profils.Add(newUser);
                db.SaveChanges();

                return Json(new { success = true, message = "Compte opérateur créé avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ChangeOperateurPassword(decimal operateurId, string newPassword)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Accès non autorisé" });

                var user = db.Profils.FirstOrDefault(l => l.Id_Operateur == operateurId);
                if (user == null)
                    return Json(new { success = false, message = "Compte opérateur non trouvé" });

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                db.SaveChanges();

                return Json(new { success = true, message = "Mot de passe modifié avec succès" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteOperateurAccount(decimal operateurId)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Accès non autorisé" });

                var user = db.Profils.FirstOrDefault(l => l.Id_Operateur == operateurId);
                if (user == null)
                    return Json(new { success = false, message = "Compte opérateur non trouvé" });

                db.Profils.Remove(user);
                db.SaveChanges();

                return Json(new { success = true, message = "Compte opérateur supprimé" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteUser(int userId)
        {
            try
            {
                if (!IsAdmin())
                    return Json(new { success = false, message = "Accès non autorisé" });

                var user = db.Profils.Find(userId);
                if (user == null)
                    return Json(new { success = false, message = "Utilisateur non trouvé" });

                if (user.Username == Session["user"]?.ToString())
                    return Json(new { success = false, message = "Vous ne pouvez pas supprimer votre propre compte" });

                db.Profils.Remove(user);
                db.SaveChanges();
                return Json(new { success = true, message = "Utilisateur supprimé" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur: " + ex.Message });
            }
        }
    }
}