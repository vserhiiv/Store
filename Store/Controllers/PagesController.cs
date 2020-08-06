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
            // Получаем/Устанавливаем краткий заголовок (slug)
            if (page == "")
                page = "home";

            // Объявляем модель и класс DTO
            PageVM model;
            PagesDTO dto;

            // Проверяем, доступна ли страница
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
            }

            // Получаем DTO страницы
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            // Устанавливаем заголовок страницы (title)
            ViewBag.PageTitle = dto.Title;

            // Проверяем боковую панель
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            // Зоплняем модель данными
            model = new PageVM(dto);

            // Возвращаем представление с моделью
            return View(model);
        }

        public  ActionResult PagesMenuPartial()
        {
            // Инициализируем лист PageVM
            List<PageVM> pageVMList;

            // Получаем все страницы, кроме home
            using (Db db =new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x)).ToList();

            }

            // Возвращаем частичное представление с листом данных
            return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            // Объявляем модель
            SidebarVM model;

            // Инициализируем модель данными
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            }

            // Возвращаем модель в частичное представление
            return PartialView("_SidebarPartial", model);
        }
    }
}