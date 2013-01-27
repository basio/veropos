﻿using System;
using System.Linq;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Printing;

namespace Samba.Services.Printing
{
    public class SlipPrinterJob : AbstractPrintJob
    {
        public SlipPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            lines = PrinterHelper.AlignLines(lines, Printer.CharsPerLine, true).ToArray();
            lines = PrinterHelper.ReplaceChars(lines, Printer.ReplacementPattern).ToArray();

            var printer = new LinePrinter(Printer.ShareName, Printer.CharsPerLine, Printer.CodePage);

            printer.StartDocument();

            foreach (var s in lines)
            {
                SendToPrinter(printer, s);
            }

            if (lines.Length >= 2)
                printer.Cut();
            printer.EndDocument();
            _lastHeight = 0;
            _lastWidth = 0;
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document,Printer.CharsPerLine));
        }

        private static int _lastWidth;
        private static int _lastHeight;

        private static void SendToPrinter(LinePrinter printer, string line)
        {
            if (!string.IsNullOrEmpty(line.Trim()))
            {
                if (line.Length > 3 && Char.IsNumber(line[2]) && Char.IsNumber(line[3]))
                {
                    _lastHeight = Convert.ToInt32(line[2].ToString());
                    _lastWidth = Convert.ToInt32(line[3].ToString());
                }

                if (line.StartsWith("<T>"))
                    printer.PrintCenteredLabel(RemoveTag(line), true);
                else if (line.StartsWith("<L"))
                    printer.WriteLine(RemoveTag(line), _lastHeight, _lastWidth);
                else if (line.StartsWith("<C"))
                    printer.WriteLine(RemoveTag(line), _lastHeight, _lastWidth);
                else if (line.StartsWith("<R"))
                    printer.WriteLine(RemoveTag(line), _lastHeight, _lastWidth);
                else if (line.StartsWith("<J"))
                    printer.WriteLine(RemoveTag(line), _lastHeight, _lastWidth);
                else if (line.StartsWith("<F") && line.Length > 3)
                    printer.PrintFullLine(line[3]);
                else if (line.StartsWith("<EB"))
                    printer.EnableBold();
                else if (line.StartsWith("<DB"))
                    printer.DisableBold();
                else if (line.StartsWith("<BMP"))
                    printer.PrintBitmap(RemoveTag(line));
                else if (line.StartsWith("<CUT"))
                    printer.Cut();
                else if (line.StartsWith("<BEEP"))
                    printer.Beep();
                else if (line.StartsWith("<DRAWER"))
                    printer.OpenCashDrawer();
                else if (line.StartsWith("<B"))
                    printer.Beep((char)_lastHeight, (char)_lastWidth);
                else if (line.StartsWith("<XCT") && line.EndsWith(">"))
                    printer.ExecCommand(line.Substring(4, line.Length - 5));
                else printer.WriteLine(line);
            }
        }
    }
}
