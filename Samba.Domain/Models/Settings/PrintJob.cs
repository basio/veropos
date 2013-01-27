﻿using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public enum WhenToPrintTypes
    {
        Manual,
        NewLinesAdded,
        Paid
    }

    public enum WhatToPrintTypes
    {
        Everything,
        NewLines,
        GroupedByBarcode,
        GroupedByGroupCode,
        GroupedByTag,
        LastLinesByPrinterLineCount,
        LastPaidItems
    }

    public class PrintJob : IEntity, IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ButtonText { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }

        public bool AutoPrintIfCash { get; set; }
        public bool AutoPrintIfCreditCard { get; set; }
        public bool AutoPrintIfTicket { get; set; }
        public int WhenToPrint { get; set; }
        public int WhatToPrint { get; set; }
        public bool LocksTicket { get; set; }
        public bool CloseTicket { get; set; }
        public bool UseFromPaymentScreen { get; set; }
        public bool UseFromTerminal { get; set; }
        public bool UseFromPos { get; set; }
        public bool UseForPaidTickets { get; set; }
        public bool ExcludeVat { get; set; }

        private IList<PrinterMap> _printerMaps;
        public virtual IList<PrinterMap> PrinterMaps
        {
            get { return _printerMaps; }
            set { _printerMaps = value; }
        }

        public PrintJob()
        {
            _printerMaps = new List<PrinterMap>();
            UseFromPos = true;
            UseFromPaymentScreen = true;
            CloseTicket = true;
        }
    }
}
