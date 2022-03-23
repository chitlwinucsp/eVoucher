using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ReqEvoucherStatus
    {
        public int EvoucherId { get; set; }

        public bool Status { get; set; }
    }
}
