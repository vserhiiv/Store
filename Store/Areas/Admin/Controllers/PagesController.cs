using Store.Models.Data;
using Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        [Authorize(Roles = "Admin")]
        // GET: Admin/Pages
        public ActionResult Index()
        {            
            List<PageVM> pageList;

            // Initializing the list (Db)
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            return View(pageList);
        }

        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                // Declaring a variable for short description (slug)
                string slug;

                PagesDTO dto = new PagesDTO();

                // Assigning a title to the model
                dto.Title = model.Title.ToUpper();

                // Check: if there is a short description, if not - assign
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                // Make sure the title and short description are unique
                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                // Assigning the Remaining Values to the Model
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                // Save the model to the database
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            // send a message about the success of the operation via TempData
            TempData["SM"] = "You have added a new page!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                // Get the page
                PagesDTO dto = db.Pages.Find(id);

                // Checking if the page is available
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                // Initializing the model with data
                model = new PageVM(dto);

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Get the Page ID
                int id = model.Id;

                // Declaring a short title variable
                string slug = "home";

                // Get the page by ID
                PagesDTO dto = db.Pages.Find(id);

                // Assigning a name from the resulting model to the DTO
                dto.Title = model.Title;

                // Checking the short title and assigning it if necessary
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                // Checking Title and Slug for uniqueness
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist.");
                    return View(model);
                }
                else if(db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug already exist.");
                    return View(model);
                }

                // Writing the rest of the values to the DTO class
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                db.SaveChanges();

            }
            // SM - successful message
            TempData["SM"] = "You have edited the page.";

            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            //Объявляем модель PageVM
            PageVM model;


            using (Db db = new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //Подтверждаем, что страница доступна
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //Присваивает модели информацию из базы
                model = new PageVM(dto);
            }
            //Возвращаем модель в представление
            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                db.Pages.Remove(dto);

                db.SaveChanges();

            }

            //Добавляем сообщение об успешном удалении
            TempData["SM"] = "You have deleted a page!";

            //Переадресовываем пользователя
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                // Implementing the initial counter
                int count = 1;

                // Initializing the data model
                PagesDTO dto;

                // Set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }

            }
        }

        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;

            using (Db db = new Db())
            {
                // Getting data from DTO
                SidebarDTO dto = db.Sidebars.Find(1); //hardcode

                // Filling the model with data
                model = new SidebarVM(dto);

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                // Getting data from DTO
                // SidebarDTO dto = db.Sidebars.Find(1); //Test
                SidebarDTO dto = db.Sidebars.FirstOrDefault(x => x.Id == model.Id);

                dto.Body = model.Body;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited the sidebar!";

            return RedirectToAction("EditSidebar");
        }
    }
}