using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.Models.ViewModels.Cart
{
    public class CartVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantuty { get; set; }
        public decimal Price { get; set; }
        public decimal Total 
        { 
            get { return Quantuty * Price; } 
        }
        public string Image { get; set; }
    }
}