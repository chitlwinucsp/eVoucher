using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class Purchase_History
    {
        public int id { get; set; }

        public int voucher_id { get; set; }

        public string phone_no { get; set; }

        public int qty { get; set; }

        public decimal cost { get; set; }

        public decimal discount { get; set; }

        public decimal total_cost { get; set; }

        public bool used { get; set; }

        public DateTime created_date { get; set; } = DateTime.Now;

        public DateTime updated_date { get; set; } = DateTime.Now;
    }
}
