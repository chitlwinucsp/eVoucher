using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ReqGiftToOthers
    {
        public string name { get; set; }

        public string phone_no { get; set; }

        public int git_per_user_limit { get; set; }

        public int max_limit { get; set; }
    }
}
