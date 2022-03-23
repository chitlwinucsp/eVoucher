using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace eVoucher.Models
{
    public class ReqCheckOut
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Phone_no { get; set; }

        public int EVoucherId { get; set; }

        public int Qty { get; set; }
    }
}
