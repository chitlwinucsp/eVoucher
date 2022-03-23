using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ResCheckOut
    {
        public ResultInfo ResultInfo { get; set; }

        public int TransactionId { get; set; }

        public int EVoucherId { get; set; }

        public string EvoucherTitle { get; set; }

        public int Qty { get; set; }

        public decimal Cost { get; set; }

        public decimal Discount { get; set; }

        public decimal Total_Cost { get; set; }
    }
}
