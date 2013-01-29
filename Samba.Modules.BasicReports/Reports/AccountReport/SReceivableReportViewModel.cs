using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Samba.Localization.Properties;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    public class SReceivableReportViewModel : SupplierReportViewModel
    {
        protected override FlowDocument GetReport()
        {
            return CreateReport(Resources.AccountsReceivable, true, false);
        }

        protected override string GetHeader()
        {
            return Resources.Supplier + " "+
            Resources.AccountsReceivable;
        }
    }
}
