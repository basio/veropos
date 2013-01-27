﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Printing;

namespace Samba.Services.Printing
{
    class DemoPrinterJob : AbstractPrintJob
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        public DemoPrinterJob(Printer printer)
            : base(printer)
        {
        }

        public override void DoPrint(string[] lines)
        {
            Debug.Assert(!string.IsNullOrEmpty(Printer.ShareName));

            lines = PrinterHelper.AlignLines(lines, Printer.CharsPerLine, false).ToArray();
            lines = PrinterHelper.ReplaceChars(lines, Printer.ReplacementPattern).ToArray();
            var text = lines.Aggregate("", (current, s) => current + RemoveTagFmt(s));
            if (!IsValidFile(Printer.ShareName) || !SaveToFile(Printer.ShareName, text))
                SendToNotepad(Printer, text);
        }

        private static bool IsValidFile(string fileName)
        {
            fileName = fileName.Trim();
            if (fileName == "." || !fileName.Contains(".")) return false;
            var result = false;
            try
            {
                new FileInfo(fileName);
                result = true;
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (NotSupportedException)
            {
            }
            return result;
        }

        private static bool SaveToFile(string fileName, string text)
        {
            try
            {
                File.WriteAllText(fileName, text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void SendToNotepad(Printer printer, string text)
        {
            var pcs = printer.ShareName.Split('#');
            var wname = "Edit";
            if (pcs.Length > 1) wname = pcs[1];

            var notepads = Process.GetProcessesByName(pcs[0]);

            if (notepads.Length == 0)
                notepads = Process.GetProcessesByName("notepad");

            if (notepads.Length == 0) return;

            if (notepads[0] != null)
            {
                IntPtr child = FindWindowEx(notepads[0].MainWindowHandle, new IntPtr(0), wname, null);
                SendMessage(child, 0x000C, 0, text);
            }
        }

        private static string RemoveTagFmt(string s)
        {
            var result = RemoveTag(s.Replace("|", " "));
            if (!string.IsNullOrEmpty(result)) return result + "\r\n";
            return "";
        }

        public override void DoPrint(FlowDocument document)
        {
            DoPrint(PrinterTools.FlowDocumentToSlipPrinterFormat(document, Printer.CharsPerLine));
        }
    }
}
