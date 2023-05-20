using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string AccountNumber { get; set; }
        public string Password  { get; set; }
        public string ConfirmPassword { get; set; }
        public decimal Balance { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
