using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Domain.Models.Suppliers;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
     public  abstract class SupplierReportViewModel : ReportViewModelBase
    {
     
        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected IEnumerable<SupplierData> GetBalancedAccounts()
        {
           
            var transactions = Dao.Query<CashTransaction>().Where(x => x.SupplierId > 0);
            var transactionSum = transactions.GroupBy(x => x.SupplierId).Select(
                x =>
                new
                {
                    SupplierId = x.Key,
                    Amount = x.Sum(y => y.TransactionType == 1 ? y.Amount : 0 - y.Amount)
                }).ToList();

            var accountTransactions = Dao.Query<AccountTransaction>().Where(x => x.SupplierId > 0);
            var accountTransactionSum = accountTransactions.GroupBy(x => x.SupplierId).Select(
                x =>
                new
                {
                    SupplierId = x.Key,
                    Amount = x.Sum(y => y.TransactionType == 3 ? y.Amount : 0 - y.Amount)
                }).ToList();


            var  supplierIds = transactionSum.Select(x => x.SupplierId).Distinct();
            supplierIds = supplierIds.Union(accountTransactionSum.Select(x => x.SupplierId).Distinct());

            var list = (from supplierId in supplierIds
                        let amount = transactionSum.Where(x => x.SupplierId == supplierId).Sum(x => x.Amount)
                        let account = accountTransactionSum.Where(x => x.SupplierId == supplierId).Sum(x => x.Amount)
                        select new { SupplierId = supplierId, Amount = (amount + account ) })
                            .Where(x => x.Amount != 0).ToList();

            var sids = list.Select(x => x.SupplierId).ToList();

            var accounts = Dao.Select<Supplier, SupplierData>(
                    x => new SupplierData { Id = x.Id, SupplierName = x.Name, PhoneNumber = x.PhoneNumber, Amount = 0 },
                    x => sids.Contains(x.Id));

            foreach (var accountData in accounts)
            {
                accountData.Amount = list.SingleOrDefault(x => x.SupplierId == accountData.Id).Amount;
            }

            return accounts;
        }

        public FlowDocument CreateReport(string reportHeader, bool? returnReceivables, bool selectInternalAccounts)
        {
            var report = new SimpleReport("8cm");
            report.AddHeader("Vero");
            report.AddHeader(reportHeader);
            report.AddHeader(string.Format(Resources.As_f, DateTime.Now));

            var accounts = GetBalancedAccounts();
            if (returnReceivables != null)
                accounts = returnReceivables.GetValueOrDefault(false) ?
                                accounts.Where(x => x.Amount < 0) :
                                accounts.Where(x => x.Amount > 0);

            report.AddColumTextAlignment("Tablo", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Tablo", "35*", "35*", "30*");


            if (accounts.Count() > 0)
            {
                report.AddTable("Tablo", Resources.Accounts, "", "");

                var total = 0m;
                foreach (var account in accounts)
                {
                    total += Math.Abs(account.Amount);
                    report.AddRow("Tablo", account.PhoneNumber, account.SupplierName, Math.Abs(account.Amount).ToString(ReportContext.CurrencyFormat));
                }
                report.AddRow("Tablo", Resources.GrandTotal, "", total);
            }
            else
            {
                report.AddHeader(string.Format(Resources.NoTransactionsFoundFor_f, reportHeader));
            }

            return report.Document;
        }
    }
}
