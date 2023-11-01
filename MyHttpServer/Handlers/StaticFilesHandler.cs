using MyHttpServer.tempOfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Handlers;


public class StaticFileHandlers : Handler
{
    public AppSettingsConfig config;

    public StaticFileHandlers(AppSettingsConfig config)
    {
        this.config = config;
    }

    public override void HandleRequest(HttpListenerContext context)
    {
        // некоторая обработка запроса
        var request = context.Request;
        using var response = context.Response;
        var absoluteRequestUrl = request.Url!.AbsolutePath;
        var pathOfStaticFile = Path.Combine(config.StaticPathFiles, absoluteRequestUrl.Trim('/'));


        if (absoluteRequestUrl!.Split('/')!.LastOrDefault()!.Contains('.'))
        {
            var pattern = absoluteRequestUrl?.Split('/')?.LastOrDefault();
            pattern = pattern?[pattern.IndexOf('.')..];
            if (File.Exists(pathOfStaticFile) && pattern != null)
            {
                response.ContentType = DictionaryExtensions._dictOfExtenshions[pattern];
                using var fileStream = File.OpenRead(pathOfStaticFile);
                fileStream.CopyTo(response.OutputStream);
            }
            else
            {
                using var fileStream = File.OpenRead(Path.Combine(config.StaticPathFiles, "404.html"));
                fileStream.CopyTo(response.OutputStream);
            }
        }
        // передача запроса дальше по цепи при наличии в ней обработчиков
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
    }
}