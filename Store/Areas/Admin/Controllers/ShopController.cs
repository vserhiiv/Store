using PagedList;
using Store.Areas.Admin.Models.ViewModels.Shop;
using Store.Models.Data;
using Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
            return View(categoryVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            using (Db db = new Db())
            {
                // Checking the category name for uniqueness
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                CategoryDTO dto = new CategoryDTO();

                // Adding data to the model
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                // We save to the database
                db.Categories.Add(dto);
                db.SaveChanges();

                // Get the ID to return to the view
                id = dto.Id.ToString();

            }

            // Returning the ID to the view
            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;

                CategoryDTO dto;

                // Set sorting for each page
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }

            }
        }

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                CategoryDTO dto = db.Categories.Find(id);

                db.Categories.Remove(dto);

                db.SaveChanges();

            }

            TempData["SM"] = "You have deleted a category!";

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Checking the category name for uniqueness
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                CategoryDTO dto = db.Categories.Find(id);

                // Editing the DTO model
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();

            }

            return "ok";

        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            // Add a list of categories from the base to the model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Checking the product name for uniqueness
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // ProductId
            int id;

            // Initializing and saving the model based on ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            TempData["SM"] = "You have added a product!";

            #region Upload Image
            // Create the necessary directory links
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                     
            string[] paths = new string[] {
                 Path.Combine(originalDirectory.ToString(), "Products"),
                 Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()),
                 Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs"),
                 Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery"),
                 Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs")
            };

            // Check for the presence of directories (if not, create)
            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            // Checking if the file has been uploaded
            if (file != null && file.ContentLength > 0)
            {
                // Get the file extension
                string ext = file.ContentType.ToLower();

                // Checking the file extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image has not aploaded - wrong image extension!");
                        return View(model);
                    }
                }

                // We declare a variable with the name of the image
                string imageName = file.FileName;

                // Save the image name in the DTO model
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Assigning paths to the original and thumbnail image
                var path = string.Format($"{paths[1]}\\{imageName}");
                var path2 = string.Format($"{paths[2]}\\{imageName}");

                // Save the original image
                file.SaveAs(path);

                // Create and save a thumbnail copy
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            return RedirectToAction("AddProduct");
        }

        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listOfProductVM;

            // Set the page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // Initializing the List and filling it with data
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                // Filling categories with data
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Install the selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            // Setting up page navigation
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;


            return View(listOfProductVM);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductVM model;

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                // Checking if the product is available
                if (dto == null)
                {
                    return Content("That product does not exist.");
                }

                model = new ProductVM(dto);

                // Creating a list of categories
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Getting images from the gallery
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }


            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Get the product ID
            int id = model.Id;

            // Filling the list with categories and images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Checking the product name for uniqueness
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Updating the product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }


            TempData["SM"] = "You have edited the product!";


            #region Image Upload

            // Checking file upload
            if (file != null && file.ContentLength > 0)
            {

                // Get the file extension
                string ext = file.ContentType.ToLower();

                // Checking the extension 
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image has not aploaded - wrong image extension!");
                        return View(model);
                    }
                }

                // Setting download paths
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                string[] paths = new string[]
                {
                 Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()),
                 Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs")
                };

                // Remove existing files and directories
                DirectoryInfo di1 = new DirectoryInfo(paths[0]);
                DirectoryInfo di2 = new DirectoryInfo(paths[1]);

                foreach (var fileDeleted in di1.GetFiles())
                {
                    fileDeleted.Delete();
                }

                foreach (var fileDeleted in di2.GetFiles())
                {
                    fileDeleted.Delete();
                }

                // Save the name of the image
                string imageName = file.FileName;

                using (Db db =new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // We keep the original and preview version
                var path = string.Format($"{paths[0]}\\{imageName}");
                var path2 = string.Format($"{paths[1]}\\{imageName}");

                // Save the original image
                file.SaveAs(path);

                // Create and save a thumbnail copy
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop( 1, 1);
                img.Save(path2);
            }

            #endregion


            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            // Removing a product from the database
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            // Delete product categories (images)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString , true);


            TempData["SM"] = "You have deleted a product!";


            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Let's iterate over all received files
            foreach (string fileName in Request.Files)
            {
                // Initializing files
                HttpPostedFileBase file = Request.Files[fileName];

                // Checking for NULL
                if (file != null && file.ContentLength > 0)
                {
                    // Assigning paths to directories
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id + "\\Gallery\\Thumbs");
                    // Assigning image paths
                    var path = string.Format($"{pathString1}\\{file.FileName }");
                    var path2 = string.Format($"{pathString2}\\{file.FileName }");

                    // We keep the original images and small copies
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1,1);
                    img.Save(path2);
                }
            }
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        public ActionResult Orders()
        {
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Looping through the OrderVM model data
                foreach (var order in orders)
                {

                    // Initialize a product dictionary
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();


                    decimal total = 0m;

                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Get the username
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;

                    // Looping through the list of products from OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        decimal price = product.Price;

                        string productName = product.Name;

                        // Add a product to the dictionary
                        productAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQuantity = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }


            return View(ordersForAdmin);
        }
    }
}