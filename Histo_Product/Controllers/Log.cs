using Histo_Product.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace Histo_Product.Controllers
{
    public class LogController : Controller
    {
        Model1 db = new Model1();

        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.HideNavbar = true;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                var user = db.Profils.FirstOrDefault(u => u.Username == username);

                if (user == null)
                {
                    ViewBag.Error = "Nom d'utilisateur ou mot de passe incorrect";
                    return View();
                }

                bool isPasswordValid = false;

                if (user.Password.StartsWith("$2a$") || user.Password.StartsWith("$2b$"))
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                }
                else
                {
                    isPasswordValid = (user.Password == password);

                    if (isPasswordValid)
                    {
                        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                        db.SaveChanges();
                    }
                }

                if (isPasswordValid)
                {
                    Session["user"] = user.Username;
                    Session["role"] = user.Role;
                    Session["userId"] = user.Id;

                    switch (user.Role)
                    {
                        case "Administrateur":
                            return RedirectToAction("Dashboard", "Admin");
                        case "Utilisateur":
                            return RedirectToAction("Dashboard", "Product");
                        case "Operateur":
                            return RedirectToAction("Dashboard", "Operateur");
                        default:
                            return RedirectToAction("Dashboard", "Product");
                    }
                }
                else
                {
                    ViewBag.Error = "Nom d'utilisateur ou mot de passe incorrect";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Erreur de connexion: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public JsonResult ForgotPassword(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "Veuillez entrer votre email" });
                }

                var user = db.Profils.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    return Json(new { success = false, message = "Aucun compte trouvé avec cet email" });
                }

                string tempPassword = GenerateTemporaryPassword();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                user.Password = hashedPassword;
                db.SaveChanges();

               
                return Json(new
                {
                    success = true,
                    message = $"Mot de passe réinitialisé avec succès! Votre nouveau mot de passe  est : <strong>{tempPassword}</strong>"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de la réinitialisation: " + ex.Message });
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            var password = new char[10];

            for (int i = 0; i < 10; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}