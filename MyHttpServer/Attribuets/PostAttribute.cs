using MyHttpServer.Attribuets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer.Attribuete;


public class PostAttribute : Attribute, IHttpMethodAttribute
{
    public PostAttribute(string actionName)
    {
        ActionName = actionName;
    }

    public string ActionName { get; }
}