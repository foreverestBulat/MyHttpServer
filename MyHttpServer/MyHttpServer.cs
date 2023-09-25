
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace MyHttpServer;

public class HttpServer
{
    private HttpListener server;
    private Task startServer;
    private Task waitFinish;
    private HttpListenerContext context;
    private HttpListenerResponse response;
    private CancellationTokenSource cts = new();
    //private HttpListenerRequest request;
    private string path;                // = "C:/Users/Admin/Desktop/ОРИС/static/main.html";
    private StreamReader site;
    private AppSettingsConfig config;

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
        //var config = await GetConnectionConfigurationServer("appsettings.json");

        GetConfig("appsettings.json");

        server.Prefixes.Add($"{config.Address}:{config.Port}/");        // "http://127.0.0.1:2323/"
        var token = cts.Token;

        Console.WriteLine("Запуск сервера");
        server.Start();

        while (true)
        {
            context = await server.GetContextAsync();
            HttpListenerRequest request = context.Request;
            response = context.Response;

            // Получить запрашиваемый путь
            string requestedPath = request.Url.LocalPath;

            // Проверить, запрашивается ли файл CSS или изображение
            if (requestedPath.EndsWith(".css"))
            {
                // Отправить файл CSS
                SendCSSFile(requestedPath);
            }
            else if (requestedPath.StartsWith("/images/"))
            {
                // Отправить изображение
                SendImageFile(requestedPath);
            }
            else
            {
                // Запрос на другие ресурсы, обрабатываем как раньше
                CheckExistFileHTML();
                site = new StreamReader(path);

                byte[] buffer = Encoding.UTF8.GetBytes(site.ReadToEnd());
                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                 
                await output.WriteAsync(buffer);
                await output.FlushAsync();
            }

            if (!(waitFinish.Status == TaskStatus.Running))
                break;

            Console.WriteLine("Запрос обработан");
        }
        server.Stop();

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

    private void CheckExistFileHTML()
    {
        if (File.Exists($"{config.StaticPathFiles}/index.html"))
        {
            path = $"{config.StaticPathFiles}/index.html";
        }
        else
        {
            Console.WriteLine("Index.html не найден");
        }
    }

    private void GetConfig(string filename)
    {
        if (File.Exists(filename))
        {

            using (var file = new FileStream("appsettings.json", FileMode.Open))
            {
                config = JsonSerializer.Deserialize<AppSettingsConfig>(file);
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
}



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