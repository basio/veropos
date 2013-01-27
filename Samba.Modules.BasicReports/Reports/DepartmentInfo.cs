﻿using System.Linq;

namespace Samba.Modules.BasicReports.Reports
{
    internal class DepartmentInfo
    {
        public int DepartmentId { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public decimal TaxServices { get; set; }
        public int TicketCount { get; set; }
        public string DepartmentName
        {
            get
            {
                var d = ReportContext.Departments.SingleOrDefault(x => x.Id == DepartmentId);
                return d != null ? d.Name : Localization.Properties.Resources.UndefinedWithBrackets;
            }
        }
    }
}