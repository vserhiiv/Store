using Store.Models.Data;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;

            // Initializing the model with data
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }


            return PartialView("_CategoryMenuPartial", categoryVMList);
        }


        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // Initializing the list with data
                productVMList = db.Products.ToArray()
                    .Where(x => x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Get the category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }


            return View(productVMList);
        }


        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Обьявляем модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            // Инициализируем ID продукта
            int id = 0;

            using (Db db = new Db())
            {
                // Проверяем, доступен ли продукт
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Инициализируем модель DTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Получаем ID
                id = dto.Id;

                // Инициализируем модель VM данными
                model = new ProductVM(dto);
            }

            // Получаем изображения из галереи
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            // Возвращаем модель в представление
            return View("ProductDetails", model);
        }
    }
}