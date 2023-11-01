
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

            Handler staticFilesHandler = new StaticFileHandlers(config);
            Handler controllerHandler = new ControllerHandler();
            staticFilesHandler.Successor = controllerHandler;
            staticFilesHandler.HandleRequest(context);

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
                Console.WriteLine(requestedPath);
                SendHTMLFile();
            }

            if (!(waitFinish.Status == TaskStatus.Running))
                break;

            Console.WriteLine("Запрос обработан");
        }
        server.Stop();
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
    }

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