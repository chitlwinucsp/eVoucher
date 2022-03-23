using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ReqCreateEvoucher
    {
        [JsonIgnore]
        public string id { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string expiry_date { get; set; }

        public byte[] image { get; set; }

        public int amount { get; set; }

        public int payment_method { get; set; }

        public decimal discount { get; set; }

        public int qty { get; set; }

        public string buy_type { get; set; }

        public bool active { get; set; }

        public ReqBuyMyself BuyMyself { get; set; }


        public ReqGiftToOthers GiftToOthers { get; set; }
    }
}
