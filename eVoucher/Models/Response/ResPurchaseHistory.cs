using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ResPurchaseHistory
    {
        public ResultInfo ResultInfo { get; set; }

        public List<Purchase_History> Purchase_Histories { get; set; }
    }
}
