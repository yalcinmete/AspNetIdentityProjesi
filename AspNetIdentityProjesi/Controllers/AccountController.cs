using AspNetIdentityProjesi.Identity;
using AspNetIdentityProjesi.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetIdentityProjesi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        public AccountController()
        {
            var userStore = (new UserStore<ApplicationUser>(new IdentityDataContext()));
            userManager = new UserManager<ApplicationUser>(userStore);

            //userManager.PasswordValidator = new PasswordValidator()
            userManager.PasswordValidator = new CustomPasswordValidator()
            {
                RequireDigit = true, //En az 1 sayısal ifade olsun
                RequiredLength = 7, //Minimum 7 karakter olsun
                RequireLowercase = true, //mutlaka 1 tane küçük harf olsun
                RequireUppercase = true,
                RequireNonLetterOrDigit = true,//Mutlaka 1 tane alfanumerik değer olmak zorunda 
            };

            //Mail adresleri kişiye özel olması için.Tekrarlı mail adresi kullanmamak için.
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                RequireUniqueEmail = true,
                AllowOnlyAlphanumericUserNames = false //alfa numerik karakter olsun mu .
            };
        }


        [AllowAnonymous]
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            //kullanıcının gitmek istediği sayfayı tutmak için olusturduk
            ViewBag.returnUrl = returnUrl;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View("Error", new string[] { "Erişim hakkınız yok" });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = userManager.Find(model.Username, model.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Yanlış kullanıcı adı veya parola");
                }
                else
                {

                    var authManager = HttpContext.GetOwinContext().Authentication;

                    //cookie/çerez oluşturuyoruz.
                    var identity = userManager.CreateIdentity(user, "ApplicationCookie");

                    //kullanıcı hatırlansın mı ?
                    var authProperties = new AuthenticationProperties()
                    {
                        //true yerine checkbox'tan gelen değer neyse onu da eşitleyebilirsin
                        IsPersistent = true
                    };

                    authManager.SignOut();
                    authManager.SignIn(authProperties, identity);
                    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
                }
            }
            ViewBag.returnUrl = returnUrl;
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(Register model)
        {
            var user = new ApplicationUser();
            user.UserName = model.Email;
            user.Email = model.Email;

            var result = userManager.Create(user, model.Password);

            if (result.Succeeded)
            {
                userManager.AddToRole(user.Id, "User");
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error); //"" boş olması genel hatada gösterilsin.Modelden bağımsız.Validationsummary'de çıkar.
                }
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut();
            return RedirectToAction("Login");
        }
    }
}