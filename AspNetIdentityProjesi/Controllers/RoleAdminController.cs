using AspNetIdentityProjesi.Identity;
using AspNetIdentityProjesi.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace AspNetIdentityProjesi.Controllers
{
    [Authorize(Roles="Admin")]
    public class RoleAdminController : Controller
    {
        //private RoleManager<ApplicationRole> roleManager; //Ek tanımlamalar yapmak istersek (Description) ApplicationRole sınıfını da kulllabiliriz.
        private RoleManager<IdentityRole> roleManager;
        private UserManager<ApplicationUser> userManager;

        public RoleAdminController()
        {
            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new IdentityDataContext()));
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new IdentityDataContext()));
        }

        // GET: RoleAdmin
        public ActionResult Index()
        {
            return View(roleManager.Roles);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(string name)
        {
            if (ModelState.IsValid)
            {
                var result = roleManager.Create(new IdentityRole(name));

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item);
                    }
                }
            }
            return View(name);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var role = roleManager.FindById(id);
            if (role != null)
            {
                var result = roleManager.Delete(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new string[] { "Role Bulunamadı" });
            }
        }

        public ActionResult Edit(string id)
        {
            var role = roleManager.FindById(id);

            var members = new List<ApplicationUser>();//üyeler
            var nonmembers = new List<ApplicationUser>(); //üyeolmayanlar

            foreach (var user in userManager.Users.ToList())
            {
                //user role'un içindeyse members'e ekledik.Eğer user roleun içinde değilse nonmembers listesine eklendi.
                var list = userManager.IsInRole(user.Id, role.Name) ? members : nonmembers;
                list.Add(user);
            }

            return View(new RoleEditModel()
            {
                Role = role,
                Members = members,
                NonMembers = nonmembers
            });
        }

        [HttpPost]
        public ActionResult Edit(RoleUpdateModel model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                
                foreach (var userId in model.IdsToAdd ?? new string[] {})//idsToAdd null ise boş dizi oluşturduk.
                {
                    result = userManager.AddToRole(userId, model.RoleName);
                    if (!result.Succeeded) 
                    {
                        return View("Error", result.Errors);
                    }
                }

                foreach (var userId in model.IdsToDelete ?? new string[] { })//IdsToDelete null ise boş dizi oluşturduk.
                {
                    result = userManager.RemoveFromRole(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Error", result.Errors);
                    }
                }
                return RedirectToAction("Index");
            }
            return View("Error",new string[] {"Aranılan rol yok"});
        }
    }
}