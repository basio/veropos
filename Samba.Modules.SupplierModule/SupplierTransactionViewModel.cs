using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.SupplierModule
{
    public class SupplierTransactionViewModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Receivable { get; set; }
        public decimal Liability { get; set; }
        public decimal Balance { get; set; }
    }
}
