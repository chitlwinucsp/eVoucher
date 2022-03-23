using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ResPaymethodMethodList
    {
        
        public ResultInfo ResultInfo { get; set; }

        public List<payment_method> Payment_Methods { get; set; }
    }
}
