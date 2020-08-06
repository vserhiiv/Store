using Store.Models.Data;
using Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            // Обьявляем лист типа CartVM
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Проверяем, пуста ли корзина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // Складываем сумму и записываем во ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //Возвращаем лист в представление
            return View(cart);
        }
        // GET: Cart
        public ActionResult CartPartial()
        {
            // Объявляем модель CartVM
            CartVM model = new CartVM();

            // Объявляем переменную количества
            int qty = 0;

            // Объявляем переменную цены
            decimal price = 0m;

            // Проверяем сессию корзины
            if (Session["cart"] != null)
            {
                // Получаем общее количество товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantuty;
                    price += item.Quantuty * item.Price;
                }

                model.Quantuty = qty;
                model.Price = price;
            }
            else
            {
                // Или устанавливаем количество и цену в 0
                model.Quantuty = 0;
                model.Price = 0m;
            }

            // Возвращаем частичное представление с моделью
            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Обьявляем лист, параметризированный типом CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Обьявляем модель CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // Получаем продукт по ID
                ProductDTO product = db.Products.Find(id);

                // Проверяем, находится ли товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // Если нет, то добавляем товар
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    { 
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantuty = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                // Если да, добавляем единицу товара
                else
                {
                    productInCart.Quantuty++;
                }
            }

            // Получаем общее количество товаров, цену и добавляем данные в модель
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantuty;
                price += item.Quantuty * item.Price;
            }

            model.Quantuty = qty;
            model.Price = price;

            // Сохраняем состояние корзины в сессию
            Session["cart"] = cart;

            // Возвращаем частичное представление с моделью
            return PartialView("_AddToCartPartial", model);
        }

        // GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            // Обьявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Добавляем количество
                model.Quantuty++;

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantuty, price = model.Price };

                // Возвращаем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DecrementProduct (int productId)
        {

            // Обьявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Отнимаем количество
                if (model.Quantuty > 1)
                {
                    model.Quantuty--;
                }
                else
                {
                    model.Quantuty = 0;
                    cart.Remove(model);
                }

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantuty, price = model.Price };

                // Возвращаем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            // Обьявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель CartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Удаляем товар из модели
                cart.Remove(model);
            }
        }
    }
}