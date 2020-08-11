using Store.Models.Data;
using Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Store.Controllers
{
    public class CartController : Controller
    {
        public ActionResult Index()
        {
            // Declaring a list of type CartVM
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Checking if the cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            // Add up the amount and write it to the ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;


            return View(cart);
        }
        public ActionResult CartPartial()
        {
            CartVM model = new CartVM();

            // quantity
            int qty = 0;

            decimal price = 0m;

            // Checking the cart session
            if (Session["cart"] != null)
            {
                // Get the total number of goods and the price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // Or set the quantity and price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }


            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);

                // Checking if the item is already in the cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // If not, then add the product
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    { 
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                // If yes, add one item
                else
                {
                    productInCart.Quantity++;
                }
            }

            // Get the total number of products, the price and add the data to the model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save the state of the basket to the session
            Session["cart"] = cart;


            return PartialView("_AddToCartPartial", model);
        }
        public JsonResult IncrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                model.Quantity++;

                // Save the necessary data
                var result = new { qty = model.Quantity, price = model.Price };

                // Returning a JSON response with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DecrementProduct (int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // We subtract the quantity
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Save the necessary data
                var result = new { qty = model.Quantity, price = model.Price };

                // Returning a JSON response with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            // Get a list of products in the basket
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            // Get a list of products in the basket
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            string userName = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                OrderDTO orderDto = new OrderDTO();

                // Get user ID
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                // Fill the OrderDTO model with data and save
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDto);
                db.SaveChanges();


                orderId = orderDto.OrderId;

                OrderDetailsDTO orderDetailsDto = new OrderDetailsDTO();

                // Add data to the model
                foreach (var item in cart)
                {
                    orderDetailsDto.OrderId = orderId;
                    orderDetailsDto.UserId = userId;
                    orderDetailsDto.ProductId = item.ProductId;
                    orderDetailsDto.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDto);
                    db.SaveChanges();
                }
            }

            // Send an order letter to the administrator's mail
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("c97e375bc9cae2", "92ff3477fe59e8"),
                EnableSsl = true
            };
            client.Send("store@example.com", "admin@example.com", "New Order", $"You have a new order. Order number: {orderId}");

            // Resetting the session
            Session["cart"] = null;
        }

    }
}