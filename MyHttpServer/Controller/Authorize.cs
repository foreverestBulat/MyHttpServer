using MyHttpServer.Attribuete;
using MyHttpServer.Attribuets;
using MyHttpServer.Model;
using MyHttpServer.Services.EmailSender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyHttpServer.Controller
{

    [ControllerAttribute("Authorize")]
    public class AuthorizeController
    {
        [Post("SendToEmail")]
        public void SendToEmail(string emailFromUser, string passwordFromUser)
        {
            //new EmailSenderService().SendEmail(emailFromUser, passwordFromUser, "");
            //IEmailSenderService mail = EmailSenderService("bulat", emailFromUser);

            Console.WriteLine("Email has been sent.");
        }

        [Get("GetEmailList")]
        public string GetEmailList()
        {
            var htmlCode = "<html><head></head><body><h1>Вы вызвали GetEmailList</h1></body></html>";
            return htmlCode;
        }

        [Get("GetAccountsList")]
        public Account[] GetAccountsList()
        {
            var accounts = new[]
            {
            new Account(){Email = "email-1", Password = "password-1"},
            new Account(){Email = "email-2", Password = "password-2"},
        };

            return accounts;
        }
    }


    //// [Controller("/Authorize")]
    //internal class Authorize
    //{
    //    public void SendToEmail(string from, string password)
    //    {
    //        // IEmailSenderService mail = new EmailSenderService();

    //        // sendemail
    //        // mail.CreateMail(null, from, from, null, null, $"{config.NameZipFile}.zip");
    //        // mail.SendMail(config.SmtpHost, config.SmtpPort, config.EmailFrom, config.EmailPasswordKfu);

    //        //new EmailSenderService().Create
    //    }

    //    public void SendToEmail2()
    //    {
    //        //sende
    //    }

    //    public void GetEmailList()
    //    {
    //        var htmlCode = "<html>Вы вызвали GetEmailList</html>";
    //    }
    //}

    ////public class ControllerAttribute : Attribute
    ////{
        
    ////}
}
