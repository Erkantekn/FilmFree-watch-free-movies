using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace FilmFree.Controllers
{
    public class Mail_Sender
    {
        public bool SendMail(string aliciMail,string konu, string mesaj)
        {
            try
            {

                SmtpClient SmtpServer = new SmtpClient(ConfigurationManager.AppSettings["smtpServerName"]);
                var mail = new MailMessage();
                mail.From = new MailAddress(ConfigurationManager.AppSettings["senderMail"]);
                mail.To.Add(aliciMail);
                mail.Subject = konu;
                mail.IsBodyHtml = true;
                string htmlBody;
                htmlBody = mesaj;
                mail.Body = htmlBody;
                SmtpServer.Port = Convert.ToInt32(ConfigurationManager.AppSettings["portMail"]);
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["senderMail"], ConfigurationManager.AppSettings["senderPass"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                return true;
                
            }
            catch
            {
                return false;
            }
        }
    }
}