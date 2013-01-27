﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Printing;
using Samba.Persistance.Data;
using Samba.Services.Printing;

namespace Samba.Services.Test
{
    [TestClass]
    public class PrintTests
    {
        [TestMethod]
        public void CanFormatTicket()
        {
            WorkspaceFactory.SetDefaultConnectionString("c:\\testData.txt");
            IWorkspace workspace = WorkspaceFactory.Create();
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var user = new User("Emre", "1");
            workspace.Add(user);

            var menuItem1 = MenuItem.Create();
            menuItem1.Name = "Kurufasülye";
            menuItem1.Portions[0].Price.Amount = 5;

            var menuItem2 = MenuItem.Create();
            menuItem2.Name = "Pilav";
            menuItem2.Portions[0].Price.Amount = 3;

            menuItem2.AddPortion("Az", 1, "TL");

            workspace.Add(menuItem1);
            workspace.Add(menuItem2);

            var d = new Department();
            var ticket = Ticket.Create(d);

            ticket.AddTicketItem(user.Id, menuItem1, "Normal");
            ticket.AddTicketItem(user.Id, menuItem2, "Normal");
            ticket.Date = new DateTime(2010, 1, 1);
            ticket.AddTicketDiscount(DiscountType.Amount, 1, user.Id);

            var template = new PrinterTemplate();

            template.HeaderTemplate = @"SAMBA
Adisyon Tarihi:{ADİSYON TARİH}
[Müşteri Adı:
{MÜŞTERİ ADI}]";
            template.LineTemplate = @"{MİKTAR} {ÜRÜN} {FİYAT}";
            template.FooterTemplate = @"{VARSA İSKONTO}
[<C>İkram: {TOPLAM İKRAM}, teşekkürler]
[Toplam: {TOPLAM BAKİYE}]";

            var formatResult = TicketFormatter.GetFormattedTicket(ticket, ticket.GetUnlockedLines(), template);

            var expectedResult = @"SAMBA
Adisyon Tarihi:01.01.2010
1 Kurufasülye 5,00
1 Pilav 3,00
Belge TOPLAMI:|8,00
<J>İskonto:|1,00
Toplam: 7,00";

            var result = string.Join("\r\n", formatResult);

            Assert.IsTrue(result == expectedResult);

            template.MergeLines = true;
            formatResult = TicketFormatter.GetFormattedTicket(ticket, ticket.GetUnlockedLines(), template);
            result = string.Join("\r\n", formatResult);
            Assert.AreEqual(expectedResult, result);

            var l1 = ticket.AddTicketItem(user.Id, menuItem1, "Normal");
            l1.Quantity = 5;
            var l2 = ticket.AddTicketItem(user.Id, menuItem2, "Az");
            formatResult = TicketFormatter.GetFormattedTicket(ticket, ticket.GetUnlockedLines(), template);
            result = string.Join("\r\n", formatResult);
            expectedResult = @"SAMBA
Adisyon Tarihi:01.01.2010
1 Pilav 3,00
6 Kurufasülye 5,00
1 Pilav.Az 1,00
Belge TOPLAMI:|34,00
<J>İskonto:|1,00
Toplam: 33,00";

            Assert.AreEqual(expectedResult, result);

            var c = new Customer { Name = "Emre EREN" };
            workspace.Add(c);

            ticket.CustomerId = c.Id;
            ticket.CustomerName = c.Name;

            ticket.AddTicketDiscount(DiscountType.Amount, 0, user.Id);
            formatResult = TicketFormatter.GetFormattedTicket(ticket, ticket.GetUnlockedLines(), template);

            expectedResult = @"SAMBA
Adisyon Tarihi:01.01.2010
Müşteri Adı:
Emre EREN
1 Pilav 3,00
6 Kurufasülye 5,00
1 Pilav.Az 1,00
Toplam: 34,00";

            result = string.Join("\r\n", formatResult);
            Assert.IsTrue(result == expectedResult);

            l2.Gifted = true;
            template.GiftLineTemplate = "{MİKTAR} {ÜRÜN} İKRAM";

            expectedResult = @"SAMBA
Adisyon Tarihi:01.01.2010
Müşteri Adı:
Emre EREN
1 Pilav 3,00
6 Kurufasülye 5,00
1 Pilav.Az İKRAM
<C>İkram: 1,00, teşekkürler
Toplam: 33,00";

            formatResult = TicketFormatter.GetFormattedTicket(ticket, ticket.GetUnlockedLines(), template);
            result = string.Join("\r\n", formatResult);
            Assert.IsTrue(result == expectedResult);
        }

        [TestMethod]
        public void ReplaceInBracketValues()
        {
            var input = "hello [hi test]";
            var result = TagData.ReplaceInBracketValues(input, " test", "", '[', ']');
            Assert.AreEqual("hello [hi]", result);

            input = "hello [hi test] [haaa test haha]";
            result = TagData.ReplaceInBracketValues(input, " test", "", '[', ']');
            Assert.AreEqual("hello [hi] [haaa haha]", result);
        }

        [TestMethod]
        public void CanExtractTag()
        {
            const string data = "{MİKTAR} {ÜRÜN} [Fiyat:{FİYAT}]";

            var tagData = new TagData(data, "{MİKTAR}");
            Assert.IsTrue(tagData.Tag == "{MİKTAR}");
            Assert.IsTrue(tagData.Length == "{MİKTAR}".Length);
            Assert.IsTrue(tagData.StartPos == 0);

            tagData = new TagData(data, "{ÜRÜN}");
            Assert.IsTrue(tagData.Tag == "{ÜRÜN}");
            Assert.IsTrue(tagData.Length == "{ÜRÜN}".Length);
            Assert.IsTrue(tagData.StartPos == data.IndexOf("{ÜRÜN"));
            Assert.IsTrue(tagData.DataString == "{ÜRÜN}");

            tagData = new TagData(data, "{FİYAT}");
            Assert.IsTrue(tagData.Tag == "{FİYAT}");
            Assert.IsTrue(tagData.Length == "[Fiyat:{FİYAT}]".Length);
            Assert.IsTrue(tagData.StartPos == data.IndexOf("[Fiyat:{FİYAT}]"));
            Assert.IsTrue(tagData.DataString == "[Fiyat:{FİYAT}]");
            Assert.IsTrue(tagData.Title == "Fiyat:<value>");
        }

        [TestMethod]
        public void ColumnAlignmentTest()
        {
            var list = new List<string>();
            list.Add("<J00>- 1 Şırdan Tuzlama|  8,00|  8,00");
            list.Add("<J00>- 1 Ayak Paça|  8,00|  8,00");
            list.Add("<J00>- 1 Kelle Paça|  15,00|  15,00");
            list.Add("<J00>- 7 İşkembe|  6,50|  45,50");
            list.Add("<J00>- 1 İşkembe Tuzlama|  8,00|  8,00");
            list.Add("HELLO");
            list.Add("<J00>12 |  12 |  12|  8,00");
            list.Add("<J00>12 |  12 |  12|  18,00");


            var result = PrinterHelper.AlignLines(list, 42, false).ToList();

            Assert.AreEqual("- 1 Şırdan Tuzlama             8,00   8,00", result[0]);
            Assert.AreEqual("- 1 Ayak Paça                  8,00   8,00", result[1]);
            Assert.AreEqual("- 1 Kelle Paça                15,00  15,00", result[2]);
            Assert.AreEqual("- 7 İşkembe                    6,50  45,50", result[3]);
            Assert.AreEqual("- 1 İşkembe Tuzlama            8,00   8,00", result[4]);
            Assert.AreEqual("12                           12  12   8,00", result[6]);
            Assert.AreEqual("12                           12  12  18,00", result[7]);
        }
    }
}
