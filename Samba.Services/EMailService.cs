﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Samba.Services
{
    public static class EMailService
    {
        public static void SendEmail(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string fromEmailAddress, string subject, string body, string fileName, bool deleteFile, bool bypassSslErrors)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(smtpServerAddress);
            try
            {
                mail.From = new MailAddress(fromEmailAddress);
                mail.To.Add(toEmailAddress);
                mail.Subject = subject;
                mail.Body = body;

                if (!string.IsNullOrEmpty(fileName))
                    fileName.Split(',').ToList().ForEach(x => mail.Attachments.Add(new Attachment(x)));

                smtpServer.Port = smtpPort;
                smtpServer.Credentials = new NetworkCredential(smtpUser, smtpPassword);
                smtpServer.EnableSsl = true;
                if (bypassSslErrors)
                    ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                smtpServer.Send(mail);
            }
            catch (Exception e)
            {
                AppServices.LogError(e);
            }
            finally
            {
                if (deleteFile && !string.IsNullOrEmpty(fileName))
                {
                    fileName.Split(',').ToList().ForEach(
                        x =>
                        {
                            if (File.Exists(x))
                            {
                                try
                                {
                                    File.Delete(x);
                                }
                                catch (Exception) { }
                            }
                        });
                }
            }
        }

        public static void SendEMailAsync(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string fromEmailAddress, string subject, string body, string fileName, bool deleteFile, bool byPassSslErrors)
        {
            var thread = new Thread(() => SendEmail(smtpServerAddress, smtpUser, smtpPassword, smtpPort, toEmailAddress, fromEmailAddress, subject, body, fileName, deleteFile, byPassSslErrors));
            thread.Start();
        }
    }
}
