using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            // Get/Set the short title (slug)
            if (page == "")
                page = "home";


            PageVM model;
            PagesDTO dto;

            // Checking if the page is available
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
            }

            // Getting the DTO of the page
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }


            ViewBag.PageTitle = dto.Title;

            // Checking the sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            // Filling the model with data
            model = new PageVM(dto);


            return View(model);
        }

        public  ActionResult PagesMenuPartial()
        {
            List<PageVM> pageVMList;

            // Get all pages except home
            using (Db db =new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x)).ToList();

            }


            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            SidebarVM model;

            // Initializing the model with data
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1); //harcode

                model = new SidebarVM(dto);
            }

            return PartialView("_SidebarPartial", model);
        }
    }
}