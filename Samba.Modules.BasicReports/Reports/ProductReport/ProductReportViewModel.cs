﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Modules.BasicReports.Reports.ProductReport
{
    public class ProductReportViewModel : ReportViewModelBase
    {
        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");

            AddDefaultReportHeader(report, ReportContext.CurrentWorkPeriod, Resources.ItemSalesReport);

            var menuGroups = MenuGroupBuilder.CalculateMenuGroups(ReportContext.Tickets, ReportContext.MenuItems);

            report.AddColumTextAlignment("ÜrünGrubu", TextAlignment.Center, TextAlignment.Center, TextAlignment.Center);
            report.AddColumnLength("ÜrünGrubu", "40*", "Auto", "35*");
            report.AddTable("ÜrünGrubu", Resources.SalesByItemGroup, "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("ÜrünGrubu", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.Rate),
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat));
            }

            report.AddRow("ÜrünGrubu", Resources.Total, "", menuGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));


            //----------------------

            report.AddColumTextAlignment("ÜrünGrubuMiktar", TextAlignment.Center, TextAlignment.Center, TextAlignment.Center);
            report.AddColumnLength("ÜrünGrubuMiktar", "40*", "Auto", "35*");
            report.AddTable("ÜrünGrubuMiktar", Resources.QuantitiesByItemGroup, "", "");

            foreach (var menuItemInfo in menuGroups)
            {
                report.AddRow("ÜrünGrubuMiktar", menuItemInfo.GroupName,
                    string.Format("%{0:0.00}", menuItemInfo.QuantityRate),
                    menuItemInfo.Quantity.ToString("#"));
            }

            report.AddRow("ÜrünGrubuMiktar", Resources.Total, "", menuGroups.Sum(x => x.Quantity).ToString("#"));


            //----------------------

            var menuItems = MenuGroupBuilder.CalculateMenuItems(ReportContext.Tickets, ReportContext.MenuItems)
                .OrderByDescending(x => x.Quantity);

            report.AddColumTextAlignment("ÜrünTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
            report.AddColumnLength("ÜrünTablosu", "50*", "Auto", "25*");
            report.AddTable("ÜrünTablosu",  Resources.Amount,Resources.Quantity, Resources.MenuItem);

            foreach (var menuItemInfo in menuItems)
            {
                report.AddRow("ÜrünTablosu",
                    menuItemInfo.Amount.ToString(ReportContext.CurrencyFormat),
                    string.Format("{0:0.##}", menuItemInfo.Quantity),
                    
                    menuItemInfo.Name);
            }

            report.AddRow("ÜrünTablosu",  menuItems.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat),"",Resources.Total);


            //----------------------


            PrepareModificationTable(report, x => x.Voided, Resources.Voids);
            PrepareModificationTable(report, x => x.Gifted, Resources.Gifts);

            var discounts = ReportContext.Tickets
                .SelectMany(x => x.Discounts.Select(y => new { x.TicketNumber, y.UserId, Amount = y.DiscountAmount }))
                .GroupBy(x => new { x.TicketNumber, x.UserId }).Select(x => new { x.Key.TicketNumber, x.Key.UserId, Amount = x.Sum(y => y.Amount) });

            if (discounts.Count() > 0)
            {
                report.AddColumTextAlignment("İskontolarTablosu", TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("İskontolarTablosu", "20*", "Auto", "35*");
                report.AddTable("İskontolarTablosu", Resources.Discounts, "", "");

                foreach (var discount in discounts.OrderByDescending(x => x.Amount))
                {
                    report.AddRow("İskontolarTablosu", discount.TicketNumber, ReportContext.GetUserName(discount.UserId), discount.Amount.ToString(ReportContext.CurrencyFormat));
                }

                if (discounts.Count() > 1)
                    report.AddRow("İskontolarTablosu", Resources.Total, "", discounts.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            }

            //----------------------

            var ticketGroups = ReportContext.Tickets
                .GroupBy(x => new { x.DepartmentId })
                .Select(x => new { x.Key.DepartmentId, TicketCount = x.Count(), Amount = x.Sum(y => y.GetSumWithoutTax()) });

            if (ticketGroups.Count() > 0)
            {

                report.AddColumTextAlignment("AdisyonlarTablosu", TextAlignment.Left, TextAlignment.Right, TextAlignment.Right);
                report.AddColumnLength("AdisyonlarTablosu", "40*", "20*", "40*");
                report.AddTable("AdisyonlarTablosu", Resources.Tickets, "", "");

                foreach (var ticketGroup in ticketGroups)
                {
                    report.AddRow("AdisyonlarTablosu", ReportContext.GetDepartmentName(ticketGroup.DepartmentId), ticketGroup.TicketCount.ToString("#.##"), ticketGroup.Amount.ToString(ReportContext.CurrencyFormat));
                }

                if (ticketGroups.Count() > 1)
                    report.AddRow("AdisyonlarTablosu", Resources.Total, ticketGroups.Sum(x => x.TicketCount).ToString("#.##"), ticketGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
            }

            //----------------------

            var properties = ReportContext.Tickets
                .SelectMany(x => x.TicketItems.Where(y => y.Properties.Count > 0))
                .SelectMany(x => x.Properties.Where(y => y.MenuItemId == 0).Select(y => new { y.Name, x.Quantity }))
                .GroupBy(x => new { x.Name })
                .Select(x => new { x.Key.Name, Quantity = x.Sum(y => y.Quantity) });

            if (properties.Count() > 0)
            {

                report.AddColumTextAlignment("ÖzelliklerTablosu", TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength("ÖzelliklerTablosu", "60*", "40*");
                report.AddTable("ÖzelliklerTablosu", Resources.Properties, "");

                foreach (var property in properties.OrderByDescending(x => x.Quantity))
                {
                    report.AddRow("ÖzelliklerTablosu", property.Name, property.Quantity.ToString("#.##"));
                }
            }
            return report.Document;
        }

        private static void PrepareModificationTable(SimpleReport report, Func<TicketItem, bool> predicate, string title)
        {
            var modifiedItems = ReportContext.Tickets
                .SelectMany(x =>
                    x.TicketItems.Where(predicate)
                                 .OrderBy(y => y.ModifiedDateTime)
                                 .Select(y =>
                                        new
                                            {
                                                Ticket = x,
                                                UserId = y.ModifiedUserId,
                                                MenuItem = y.MenuItemName,
                                                y.Quantity,
                                                y.ReasonId,
                                                y.ModifiedDateTime,
                                                Amount = y.GetItemValue()
                                            }));


            if (modifiedItems.Count() == 0) return;

            report.AddColumTextAlignment(title, TextAlignment.Left, TextAlignment.Left, TextAlignment.Left, TextAlignment.Left);
            report.AddColumnLength(title, "14*", "45*", "28*", "13*");
            report.AddTable(title, title, "", "", "");

            foreach (var voidItem in modifiedItems.GroupBy(x => x.ReasonId))
            {
                if (voidItem.Key > 0) report.AddRow(title, ReportContext.GetReasonName(voidItem.Key), "", "", "");
                foreach (var vi in voidItem)
                {
                    report.AddRow(title, vi.Ticket.TicketNumber, vi.Quantity.ToString("#.##") + " " + vi.MenuItem, ReportContext.GetUserName(vi.UserId), vi.ModifiedDateTime.ToShortTimeString());
                }
            }

            var voidGroups =
                from c in modifiedItems
                group c by c.UserId into grp
                select new { UserId = grp.Key, Amount = grp.Sum(x => x.Amount) };

            report.AddColumTextAlignment("Personel" + title, TextAlignment.Left, TextAlignment.Right);
            report.AddColumnLength("Personel" + title, "60*", "40*");
            report.AddTable("Personel" + title, string.Format(Resources.ByPersonnel_f, title), "");

            foreach (var voidItem in voidGroups.OrderByDescending(x => x.Amount))
            {
                report.AddRow("Personel" + title, ReportContext.GetUserName(voidItem.UserId), voidItem.Amount.ToString(ReportContext.CurrencyFormat));
            }

            if (voidGroups.Count() > 1)
                report.AddRow("Personel" + title, Resources.Total, voidGroups.Sum(x => x.Amount).ToString(ReportContext.CurrencyFormat));
        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override string GetHeader()
        {
            return Resources.ItemSalesReport;
        }
    }
}
