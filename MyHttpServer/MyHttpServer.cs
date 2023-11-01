
using MyHttpServer.Handlers;
using MyHttpServer.Services.EmailSender;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace MyHttpServer;

// IEmailSEnder в отельной папке сервис
// добавить параметри в конфиг json - email password senders, from name, smptServer Host and Post
// в email sendersevice подцепить в конструкторе, получать данные из конфига
// IHandler
// staticfileshandler


//  1) создать IEmailSenederServis
//  2) добавить параметры в абсетингс.джейсон, а именно emailsander, passwordsender, fromName, htttserverhost/port
//  3) в EmailsSenderService(класс) получать данные из конфига
//  4) handlings Ihandler, Handler (HTTPContext context)
//  5) StaticFilesHandler добавить и перенести сюда логика работы с папкой статик
//  6) подставить в listening StaticFilesHandler его вызвать


public class HttpServer 
{
    internal HttpListener server;
    internal Task startServer;
    internal Task waitFinish;
    internal HttpListenerContext context;
    internal HttpListenerResponse response;
    internal CancellationTokenSource cts = new();
    internal HttpListenerRequest request;
    internal string path;                // = "C:/Users/Admin/Desktop/ОРИС/static/main.html";
    public AppSettingsConfig config;

    public HttpServer()
    {
        server = new HttpListener();
    }

    public void Start()
    {
        startServer = new Task(() => Run());
        waitFinish = new Task(() => Wait());
        startServer.Start();
        waitFinish.Start();

        Task.WaitAll(new Task[] { startServer, waitFinish });
    }

    private async void Run()
    {
        GetConfig("appsettings.json");

        server.Prefixes.Add($"{config.Address}:{config.Port}/"); //($"{config.Address}:{config.Port}/");        // "http://127.0.0.1:2323/"

        Console.WriteLine("Запуск сервера");
        server.Start();

        while (true)
        {
            context = await server.GetContextAsync();

            //Handler staticFilesHandler = new StaticFileHandlers(config);
            //Handler controllerHandler = new ControllerHandler();
            //staticFilesHandler.Successor = controllerHandler;
            //staticFilesHandler.HandleRequest(context);

            request = context.Request;
            response = context.Response;

            if (request.HttpMethod == "POST")
            {
                string dataString;

                using (var reader = new StreamReader(request.InputStream))
                {
                    dataString = reader.ReadToEnd();
                }

                var datas = dataString.Split('&');
                string name = datas[0].Split('=')[1];
                string lastname = datas[1].Split('=')[1];
                string birthday = datas[2].Split('=')[1];
                string phone = datas[3].Split('=')[1];
                string toMail = datas[4].Split('=')[1];
                var subject = "Метод HOST";

                string body = $"{name}\n" +
                    $"{lastname}\n" +
                    $"{birthday}\n" +
                    $"{phone}";

                var text = Uri.UnescapeDataString(Regex.Unescape(body));

                ExistZipFile(config.StaticPathFiles, config.NameZipFile);

                IEmailSenderService mail = new EmailSenderService(name, config.EmailFrom);
                mail.CreateMail(toMail, subject, text, $"{config.NameZipFile}.zip"); //config.EmailFrom
                mail.SendMail(config.SmtpHost, config.SmtpPort, config.EmailPassword);

                Console.WriteLine("Анкета отправлена на почту Додо");
            }

            // Получить запрашиваемый путь
            //Console.WriteLine(request.Url!.AbsolutePath);
            string requestedPath = request.Url.LocalPath;

            // Проверить, запрашивается ли файл CSS или изображение
            if (requestedPath.EndsWith(".css"))
            {
                // Отправить файл CSS
                SendCSSFile(requestedPath);
                Console.WriteLine(requestedPath);
            }
            else if (requestedPath.StartsWith("/images/"))
            {
                // Отправить изображение
                SendImageFile(requestedPath);
            }
            else
            {
                // Отправка файл HTML
                //var pathOfIndex = Path.Combine(config.StaticPathFiles, "index.html");
                //Console.WriteLine(pathOfIndex);
                Console.WriteLine(requestedPath);
                SendHTMLFile();
            }

            if (!(waitFinish.Status == TaskStatus.Running))
                break;

            Console.WriteLine("Запрос обработан");
        }
        server.Stop();
    }

    public static void ParsingSteam()
    {
        MyDataContext db = new MyDataContext();

        //Uri uri = new Uri("https://steamcommunity.com/market/");
        //Regex reHref = new Regex(@"<a[^>]+href=""([^""]+)""[^>]+>");
        //string html = new WebClient().DownloadString(uri);
        //foreach (Match match in reHref.Matches(html))
        //    Console.WriteLine(match.Groups[1].ToString());
    }


    private async void SendHTMLFile()
    {
        CheckExistFileHTML();
        StreamReader site = new StreamReader(path);
        //Console.WriteLine(site.ReadToEnd());
        byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
        response.ContentLength64 = buffer.Length;

        using Stream output = response.OutputStream;

        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    private async void SendCSSFile(string filePath)
    {
        string fullPath = Path.Combine(Environment.CurrentDirectory, config.StaticPathFiles, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            byte[] fileBytes = File.ReadAllBytes(fullPath);
            response.ContentType = "text/css";
            response.ContentLength64 = fileBytes.Length;
            using Stream outputStream = response.OutputStream;
            await outputStream.WriteAsync(fileBytes);
            await outputStream.FlushAsync();
        }
        else
        {
            // Если файл не найден, отправляем код ошибки 404 - Not Found
            response.StatusCode = 404;
            response.Close();
        }
    }

    private string GetImageContentType(string imagePath)
    {
        string extension = Path.GetExtension(imagePath).ToLower();
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".svg":
                return "image/svg+xml";
            default:
                return "application/octet-stream"; // Если формат неизвестен, отправляем общий тип содержимого
        }
    }

    private async void SendImageFile(string imagePath)
    {
        string fullPath = Path.Combine(Environment.CurrentDirectory, "static", imagePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            byte[] imageBytes = File.ReadAllBytes(fullPath);
            string contentType = GetImageContentType(fullPath);
            response.ContentType = contentType;
            response.ContentLength64 = imageBytes.Length;
            using Stream outputStream = response.OutputStream;
            await outputStream.WriteAsync(imageBytes);
            await outputStream.FlushAsync();
        }
        else
        {
            // Если файл не найден, отправляем код ошибки 404 - Not Found
            response.StatusCode = 404;
            response.Close();
        }
    }

    internal void CheckExistFileHTML()
    {
        if (File.Exists($"{config.StaticPathFiles}/index.html"))
        {
            path = $"{config.StaticPathFiles}/index.html";
        }
        else
        {
            Console.WriteLine("index.html не найден");
            response.StatusCode = 404;
            response.Close();
        }
    }

    private void GetConfig(string filename)
    {
        if (File.Exists(filename))
        {

            using (var file = new FileStream("appsettings.json", FileMode.Open))
            {
                config = System.Text.Json.JsonSerializer.Deserialize<AppSettingsConfig>(file);
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            CheckExistFolderStatic();
        }
        else
        {
            throw new FileNotFoundException(filename);
        }
    }

    private void CheckExistFolderStatic()
    {
        if (!Directory.Exists(config.StaticPathFiles))
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), config.StaticPathFiles));
                Console.WriteLine("Была создана папка static", config.StaticPathFiles);
            }
            catch
            {
                Console.WriteLine("Невозможно создать папку");
            }
        }
    }

    private void Wait() {
        while (true)
        {
            var input = Console.ReadLine();
            if (input == "stop")
                break;
        }
    }

    private void Stop()
    {
        server.Stop();
        // GC.SuppressFinalize(this);
    }

    //public void Dispose()
    //{
    //    Stop();
    //}

    private static bool ExistZipFile(string path, string putInsidePath)
    {
        if (File.Exists($"{putInsidePath}.zip")) 
            return true;
        CreateZipFile(path, putInsidePath);
        return false;
    }

    public static void CreateZipFile(string path, string putInsidePath)
    {
        ZipFile.CreateFromDirectory(@"static", $@"ZipFile.zip"); // @"static", @"ZipFile.zip"
    }

    public static void DeleteZipFile(string path)
    {
        File.Delete(path);
    }

}



//var mail = EmailSenderService.CreateMail
//    (name, config.EmailFrom, config.EmailTo, subject, body, $"{config.NameZipFile}.zip");
//EmailSenderService.SendMail
//    (config.SmtpHost, config.SmtpPort, config.EmailFrom, config.EmailPassword, mail);


// Получение JSON из тела запроса
//string jsonString;
//using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
//{
//    jsonString = reader.ReadToEnd();
//}

//// Десериализация JSON в объект
//dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

//// Выполнение необходимых действий с данными
//// ...

//Console.WriteLine(data.name);

//Response.Write("JSON получен успешно");



//if (request.HttpMethod.Equals("Post", StringComparison.OrdinalIgnoreCase) && request.Url.AbsolutePath == "/")
//{


//    // var name = request.Form["name"];

//    //using (StreamReader reader = new StreamReader(context.Request.InputStream,
//    //                                               context.Request.ContentEncoding))
//    //{
//    //    string requestBody = request.;//reader.ReadToEnd(); // Читаем тело запроса

//    //    // Десериализуем JSON-объект с отправленными данными
//    //    dynamic formData = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
//    //    string name = formData.name;
//    //    // Извлекаем значение поля input для имени




//    //    Console.WriteLine("Получено имя: " + name);
//    //    //Console.WriteLine();
//    //    // Выводим значение на консоль
//    //}
//}

//if (request.HttpMethod == "POST")
//{
//    // Прочитать данные из запроса
//    string content;
//    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
//    {
//        content = await reader.ReadToEndAsync();
//    }

//    // Вывести данные на консоль
//    Console.WriteLine(content);
//}
//if (request.HttpMethod.Equals("Post", StringComparison.OrdinalIgnoreCase) && request.Url.AbsolutePath == "/")
//{
//    var stream = new StreamReader(request.InputStream);
//    Console.WriteLine(stream.ReadToEnd());
//}

//private static async Task SendEmailAsync()
//{
//    string email = "bulatsubuh@gmail.com";

//    string smptServerHost = "smtp.gmail.com";
//    ushort smptServerPort = 587;

//    string senderEmail = "somemail@gmail.com";
//    string passwordSender = "mypassword";

//    MailAddress from = new MailAddress(email, "Bulat");
//    MailAddress to = new MailAddress(email);

//    MailMessage m = new MailMessage(from, to);
//    m.Subject = "Тест";
//    m.Body = "Письмо-тест 2 работы smtp-клиента";

//    SmtpClient smtp = new SmtpClient(smptServerHost, smptServerPort);
//    smtp.Credentials = new NetworkCredential(senderEmail, passwordSender);
//    smtp.EnableSsl = true;

//    await smtp.SendMailAsync(m);

//    Console.WriteLine("Письмо отправлено");
//}

//public static async Task SendEmailAsync()
//{
//    MailAddress from = new MailAddress("bulatsubuh@gmail.com", "Bulatic");
//    MailAddress to = new MailAddress("bulatsubuh@gmail.com");
//    MailMessage m = new MailMessage(from, to);
//    m.Subject = "Тест";
//    m.Body = "Письмо-тест 2 работы smtp-клиента";
//    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
//    smtp.Credentials = new NetworkCredential("somemail@gmail.com", "mypassword");
//    smtp.EnableSsl = true;
//    await smtp.SendMailAsync(m);
//    Console.WriteLine("Письмо отправлено");
//}



//////////////
//CheckExistFileHTML();

//site = new StreamReader(path);

//byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
//response.ContentLength64 = buffer.Length;

//using Stream output = response.OutputStream;

//await output.WriteAsync(buffer);
//await output.FlushAsync();

//if (!(waitFinish.Status == TaskStatus.Running))
//    break;

//Console.WriteLine("Запрос обработан");

//Console.WriteLine("URL: {0}", context.Request.Url.OriginalString);
//Console.WriteLine("Raw URL: {0}", context.Request.RawUrl);

//byte[] buffer = File.ReadAllBytes("static/index.html" + context.Request.RawUrl.Replace("%20", " "));

//response.ContentLength64 = buffer.Length;
//Stream st = response.OutputStream;
//st.Write(buffer, 0, buffer.Length);

//context.Response.Close();

//var requestUrl = request.Url.AbsolutePath;
//if (requestUrl.EndsWith(".html"))
//{
//    var filePath = Path.Combine(config.StaticPathFiles, requestUrl.Trim('/'));
//    if (File.Exists(filePath))
//        await DisplayFoundPageAsync(response, filePath, token);
//    else
//        DisplayNotFoundPage(response);
//}
//else if (requestUrl.EndsWith(".css"))
//{
//    var cssFilePath = Path.Combine(config.StaticPathFiles, requestUrl.Trim('/'));
//    if (File.Exists(cssFilePath))
//        await DisplayFoundCssFileAsync(response, cssFilePath, token);
//    else
//        Console.WriteLine("Css styles not founded");
//}
//else if (requestUrl.EndsWith(".jpg") || requestUrl.EndsWith(".svg") || requestUrl.EndsWith(".png"))
//{
//    var imageFilePath = Path.Combine(config.StaticPathFiles, requestUrl.Trim('/'));
//    var typeOfContent = requestUrl[requestUrl.IndexOf('.')..];
//    if (File.Exists(imageFilePath))
//        await DisplayFoundImageFileAsync(response, imageFilePath, typeOfContent, token);
//    else
//        Console.WriteLine("Image file not found");
//}

//string requestedUrl = context.Request.Url.AbsolutePath;
//string filePath = Path.Combine(baseDirectory, requestedUrl.Trim('/'));

//if (File.Exists(filePath))
//{
//    string extension = Path.GetExtension(filePath);
//    string contentType = GetContentType(extension);

//    byte[] content = File.ReadAllBytes(filePath);

//    context.Response.ContentType = contentType;
//    context.Response.ContentLength64 = content.Length;
//    context.Response.OutputStream.Write(content, 0, content.Length);
//}
//else
//{
//    // Обработка ошибки, если файл не найден
//    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
//}

//context.Response.Close();


//if (File.Exists(filePath))
//{
//    string extension = Path.GetExtension(filePath);

//    // Определяем MIME тип файла
//    string contentType = "text/plain";
//    if (extension == ".html")
//        contentType = "text/html";
//    else if (extension == ".css")
//        contentType = "text/css";

//    // Отправляем файл клиенту
//    response.ContentType = contentType;
//    using (Stream fileStream = File.OpenRead(filePath))
//    {
//        fileStream.CopyTo(response.OutputStream);
//    }
//}
//else
//{
//    // Файл не найден
//    response.StatusCode = 404;
//}
//response.Close();   


//namespace MyHttpServer;

//public class HttpServer
//{
//    private HttpListener server;
//    private Task 

//    public HttpServer()
//    {
//        server = new HttpListener();
//    }

//    public void Start()
//    {
//        server.Start();
//        Console.WriteLine();
//    }

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