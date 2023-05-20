using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumbers { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public string EndBalance { get; set; }
    }
}
