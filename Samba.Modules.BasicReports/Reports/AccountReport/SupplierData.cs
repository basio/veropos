using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
   public class SupplierData
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string SupplierName { get; set; }
        public decimal Amount { get; set; }
    }
}
