using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class voucher
    {
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public DateTime expiry_date { get; set; }

        public byte[] image { get; set; }

        public int amount { get; set; }

        public int payment_method { get; set; }

        public decimal discount { get; set; }

        public int qty { get; set; }

        public int buy_type { get; set; }

        public bool active { get; set; }

        public string user_name { get; set; }

        public string phone_no { get; set; }

        public int min_user_limit { get; set; }

        public int max_limit { get; set; }

        public DateTime created_date { get; set; } = DateTime.Now;

        public DateTime updated_date { get; set; } = DateTime.Now;



    }
}
