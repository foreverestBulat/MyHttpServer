using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Services.EmailSender
{
    interface IEmailSenderService
    {
        string Name { get; set; }
        MailAddress From { get; set; }
        MailAddress To { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        MailMessage Mail { get; set; }
        Attachment Attachment { get; set; }

        public void CreateMail(string emailTo, string subject, string body, string attachmentFilePath);
        public void SendMail(string host, int smtpPort, string pass);
    }
}
