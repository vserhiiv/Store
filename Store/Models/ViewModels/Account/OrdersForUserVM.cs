using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Store.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQuantity { get; set; }
        [DisplayName("Created at")]
        public DateTime CreatedAt { get; set; }
    }
}