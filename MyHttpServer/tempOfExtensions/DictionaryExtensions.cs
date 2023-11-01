using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.tempOfExtensions;


public static class DictionaryExtensions
{
    public static Dictionary<string, string> _dictOfExtenshions = new()
    {
        [".css"] = "text/css",
        [".html"] = "text/html",
        [".jpg"] = "image/jpeg",
        [".svg"] = "image/svg+xml",
        [".png"] = "image/png"
    };
}
