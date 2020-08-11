using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Store.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        [DisplayName("User Name")]
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQuantity { get; set; }
        [DisplayName("Created at")]
        public DateTime CreatedAt { get; set; }
    }
}