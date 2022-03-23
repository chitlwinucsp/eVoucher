using eVoucher.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eVoucher.Data
{
    public class EvoucherDBContext : DbContext
    {
        public EvoucherDBContext(DbContextOptions<EvoucherDBContext> options) : base(options)
        {

        }

        public DbSet<buy_type> Buy_Type { get; set; }

        public DbSet<payment_method> Payment_Method { get; set; }

        public DbSet<voucher> Voucher { get; set; }

        public DbSet<Purchase_History> Purchase_History { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
