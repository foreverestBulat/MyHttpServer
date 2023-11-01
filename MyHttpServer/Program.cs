using MyHttpServer;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace MyHttpServer;

public class Program
{
    public static void Main()
    {

        //HttpServer.ParsingSteam();


        var server = new HttpServer();
        server.Start();



        //var mail = EmailSenderServis.CreateMail("Bulatic", "bulatsubuh@gmail.com", "bulatsubuh@gmail.com", "subject", "body");
        //EmailSenderServis.SendMail("127.0.0.1", 587, "bulatsubuh@gmail.com", "rwha zzei jwtv bvof", mail);

        //MailAddress from = new MailAddress("bulatsubuh@gmail.com", "Bulat");
        //MailAddress to = new MailAddress("islam.bagaviev.2014@mail.ru");
        //MailMessage m = new MailMessage(from, to);
        //m.Subject = "Отправка с помощью C#";
        //m.Body = "пашли в магаз пашли в магаз пашли в магаз";
        //SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        //smtp.Credentials = new NetworkCredential("bulatsubuh@gmail.com", "rwha zzei jwtv bvof");
        //smtp.EnableSsl = true;
        //smtp.Send(m);
        //Console.WriteLine("Письмо отправлено");
    }
}

























//using (FileStream file = File.OpenRead("appsetting.json"))
//{
//    var config = JsonSerializer.Deserialize<AppSettingsConfig>(file);
//}



//var server = new HttpListener();
//// установка адресов прослушки
//server.Prefixes.Add("http://127.0.0.1:2323/");
//server.Start(); // начинаем прослушивать входящие подключения

//Task wait = new Task(() => Wait());
//wait.Start();

//Console.WriteLine(wait.Status);

//while (wait.Status == TaskStatus.Running)
//{
//    var context = await server.GetContextAsync();
//    Console.WriteLine(wait.Status);

//    var response = context.Response;

//    var path = "C:/Users/Admin/Desktop/ОРИС/static/main.html";
//    // отправляемый в ответ код htmlвозвращает
//    var site = new StreamReader(path);
//    // получаем поток ответа и пишем в него ответ

//    byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
//    response.ContentLength64 = buffer.Length;

//    using Stream output = response.OutputStream;
//    // отправляем данные
//    await output.WriteAsync(buffer);
//    await output.FlushAsync();

//    Console.WriteLine("Запрос обработан");
//    if (!(wait.Status == TaskStatus.Running))
//        break;
//}
//server.Stop();


//void Wait()
//{
//    while (true)
//    {
//        var input = Console.ReadLine();
//        if (input == "stop")
//            break;
//    }

//}










//string read;

//using (var file = new StreamReader("appsettings.json"))
//{
//    read = file.ReadToEnd();
//}

//var options = new JsonSerializerOptions
//{
//    WriteIndented = true
//};

//var js = JsonSerializer.Deserialize<AppSettingsConfig>(read, options);

//Console.WriteLine(js?.Port);


//async void WireTapping(HttpListener server)
//{

//    Console.WriteLine(213);
//    var context = await server.GetContextAsync();

//    var response = context.Response;

//    var path = "C:/Users/Admin/Desktop/ОРИС/Додо пицца/main.html";
//    // отправляемый в ответ код htmlвозвращает
//    var site = new StreamReader(path);
//    // получаем поток ответа и пишем в него ответ

//    byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
//    response.ContentLength64 = buffer.Length;

//    using Stream output = response.OutputStream;
//    // отправляем данные
//    await output.WriteAsync(buffer);
//    await output.FlushAsync();

//    Console.WriteLine("Запрос обработан");
//}

// получаем контекст

//var context = await server.GetContextAsync();

//var response = context.Response;

//var path = "C:/Users/Admin/Desktop/ОРИС/Додо пицца/main.html";
//// отправляемый в ответ код htmlвозвращает
//var site = new StreamReader(path);
//// получаем поток ответа и пишем в него ответ

//byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
//response.ContentLength64 = buffer.Length;







//using Stream output = response.OutputStream;
//// отправляем данные
//await output.WriteAsync(buffer);
//await output.FlushAsync();

//Console.WriteLine("Запрос обработан");



//namespace MyServer
//{
//    public class Program
//    {

//        static HttpListener server;
//        static HttpListenerContext context;
//        static HttpListenerResponse response;
//        static byte[] buffer;

//        public static void Main()
//        {
//            //server = new HttpListener();
//            //// установка адресов прослушки
//            //server.Prefixes.Add("http://127.0.0.1:2323/");
//            //server.Start(); // начинаем прослушивать входящие подключения

//            //// получаем контекст
//            //context = await server.GetContextAsync();

//            //response = context.Response;

//            //var path = "C:/Users/Admin/Desktop/ОРИС/Додо пицца/main.html";
//            //// отправляемый в ответ код htmlвозвращает
//            //var site = new StreamReader(path);

//            //buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
//            //// получаем поток ответа и пишем в него ответ
//            //response.ContentLength64 = buffer.Length;

//            //using Stream output = response.OutputStream;
//            //// отправляем данные
//            //await output.WriteAsync(buffer);
//            //await output.FlushAsync();

//            //Console.WriteLine("Запрос обработан");



//            //server.Stop();

//            server = new HttpListener();
//            InstallWiretappingAdress();

//            server.Start();

//            GetContext();
//            GetHTML();
//            GetThreadAnswer();

//            SendData();
//            Console.WriteLine("Запрос обработан");

//            server.Stop();  
//        }


//        public static void GetThreadAnswer()
//        {
//            response.ContentLength64 = buffer.Length;
//        }

//        public static void GetHTML(string path = "C:/Users/Admin/Desktop/ОРИС/Додо пицца/main.html")
//        {
//            var site = new StreamReader(path);
//            byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
//        }

//        public async static void GetContext()
//        {
//            context = await server.GetContextAsync();
//            response = context.Response;
//        }

//        public static void InstallWiretappingAdress()
//        {
//            server.Prefixes.Add("http://127.0.0.1:2323/");
//        }

//        public async static void SendData()
//        {
//            using Stream output = response.OutputStream;
//            await output.WriteAsync(buffer);
//            await output.FlushAsync();
//        }
//    }
//}





