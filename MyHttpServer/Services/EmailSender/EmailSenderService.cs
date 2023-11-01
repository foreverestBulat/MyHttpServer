using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHttpServer.Services.EmailSender
{
    internal class EmailSenderService : IEmailSenderService
    {
        public string Name { get; set; }
        public MailAddress From { get; set; }
        public MailAddress To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MailMessage Mail { get; set; }
        public Attachment Attachment { get; set; }

        public EmailSenderService(string name, string emailFrom)
        {
            Name = name;
            From = new MailAddress(emailFrom, name);
        }

        void IEmailSenderService.CreateMail(string emailTo, string subject, string body, string attachmentFilePath)
        {
            emailTo.Replace("%40", "@");

            string decodedEmail = HttpUtility.UrlDecode(emailTo);

            To = new MailAddress(decodedEmail);

            Mail = new MailMessage(From, To);
            Mail.BodyEncoding = Encoding.UTF8;
            Mail.Subject = Subject = subject;
            Mail.SubjectEncoding = Encoding.UTF8;
            Mail.Body = Body = body;
            Attachment = new Attachment(attachmentFilePath);
            Mail.Attachments.Add(Attachment);
            

        }

        void IEmailSenderService.SendMail(string host, int smtpPort, string pass)
        {
            SmtpClient smtpClient = new SmtpClient(host, smtpPort);
            smtpClient.Credentials = new NetworkCredential(From.User, pass);
            smtpClient.EnableSsl = true;
            smtpClient.SendMailAsync(Mail);
    }
}
